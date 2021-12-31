﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using SolastaCommunityExpansion.Helpers;
using TA;
using static SolastaModApi.DatabaseHelper.GadgetBlueprints;

namespace SolastaCommunityExpansion.Patches.GameUiScreenMap
{
    // hides certain element from the map on custom dungeons unless already discovered
    [HarmonyPatch(typeof(GameGadget), "ComputeIsRevealed")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GameGadget_ComputeIsRevealed
    {
        private static readonly GadgetBlueprint[] gadgetBlueprintsToRevealAfterDiscovery = new GadgetBlueprint[]
        {
            Exit,
            ExitMultiple,
            TeleporterIndividual,
            TeleporterParty,
            VirtualExit,
            VirtualExitMultiple,
        };

        internal static void Postfix(GameGadget __instance, ref bool ___revealed, ref bool __result)
        {
            if (!__instance.Revealed || Gui.GameLocation.UserLocation == null || !Main.Settings.EnableAdditionalIconsOnLevelMap)
            {
                return;
            }

            var userGadget = Gui.GameLocation.UserLocation.UserRooms
                .SelectMany(a => a.UserGadgets)
                .FirstOrDefault(b => b.UniqueName == __instance.UniqueNameId);

            if (userGadget == null || Array.IndexOf(gadgetBlueprintsToRevealAfterDiscovery, userGadget.GadgetBlueprint) < 0)
            {
                return;
            }

            // reverts the revealed state and recalculates it
            ___revealed = false;
            __result = false;

            var x = (int)__instance.FeedbackPosition.x;
            var y = (int)__instance.FeedbackPosition.z;

            var feedbackPosition = new int3(x, 0, y);
            var referenceBoundingBox = new BoxInt(feedbackPosition, feedbackPosition);

            var gridAccessor = GridAccessor.Default;

            foreach (var position in referenceBoundingBox.EnumerateAllPositionsWithin())
            {
                if (gridAccessor.Visited(position))
                {
                    var gameLocationService = ServiceRepository.GetService<IGameLocationService>();
                    var worldGadgets = gameLocationService.WorldLocation.WorldSectors.SelectMany(ws => ws.WorldGadgets);
                    var worldGadget = worldGadgets.FirstOrDefault(wg => wg.GameGadget == __instance);

                    var isInvisible = __instance.IsInvisible();
                    var isEnabled = __instance.IsEnabled();

                    GameLocationManager_ReadyLocation.SetTeleporterGadgetVisibility(worldGadget, isEnabled && !isInvisible);

                    ___revealed = true;
                    __result = true;

                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameGadget), "SetCondition")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GameGadget_SetCondition
    {
        internal static void Prefix(GameGadget __instance, int conditionIndex, List<string> ___conditionNames, out bool __state)
        {
            __state = false;

            if (!Main.Settings.HideExitAndTeleporterGizmosIfNotDiscovered)
            {
                return;
            }

            if(conditionIndex >= 0 && conditionIndex < ___conditionNames.Count)
            {
                var param = ___conditionNames[conditionIndex];
                if (param == GameGadgetExtensions.Enabled || param == GameGadgetExtensions.ParamEnabled)
                {
                    __state = __instance.IsEnabled();
                }
            }
        }

        internal static void Postfix(GameGadget __instance, int conditionIndex, List<string> ___conditionNames, bool __state)
        {
            if (!Main.Settings.HideExitAndTeleporterGizmosIfNotDiscovered)
            {
                return;
            }

            if (conditionIndex >= 0 && conditionIndex < ___conditionNames.Count)
            {
                var param = ___conditionNames[conditionIndex];

                if (param == GameGadgetExtensions.Enabled || param == GameGadgetExtensions.ParamEnabled)
                {
                    var newState = __instance.IsEnabled();

                    if (newState != __state)
                    {
                        var service = ServiceRepository.GetService<IGameLocationService>();
                        if (service != null)
                        {
                            var worldGadget = service.WorldLocation.WorldSectors
                                .SelectMany(ws => ws.WorldGadgets)
                                .FirstOrDefault(wg => wg.GameGadget == __instance);

                            if (worldGadget != null)
                            {
                                GameLocationManager_ReadyLocation.SetTeleporterGadgetVisibility(worldGadget, newState);
                            }
                        }
                    }
                }
            }
        }
    }
}
