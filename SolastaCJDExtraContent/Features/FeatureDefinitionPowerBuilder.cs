﻿

using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCJDExtraContent.Features
{
    public class FeatureDefinitionPowerBuilder : BaseDefinitionBuilder<FeatureDefinitionPower>
    {
        public FeatureDefinitionPowerBuilder(string name, string guid, int usesPerRecharge, RuleDefinitions.UsesDetermination usesDetermination,
            string usesAbilityScoreName,
            RuleDefinitions.ActivationTime activationTime, int costPerUse, RuleDefinitions.RechargeRate recharge,
            bool proficiencyBonusToAttack, bool abilityScoreBonusToAttack, string abilityScore,
            EffectDescription effectDescription, GuiPresentation guiPresentation) : base(name, guid)
        {
            Definition.SetFixedUsesPerRecharge(usesPerRecharge);
            Definition.SetUsesDetermination(usesDetermination);
            Definition.SetUsesAbilityScoreName(usesAbilityScoreName);
            Definition.SetActivationTime(activationTime);
            Definition.SetCostPerUse(costPerUse);
            Definition.SetRechargeRate(recharge);
            Definition.SetProficiencyBonusToAttack(proficiencyBonusToAttack);
            Definition.SetAbilityScoreBonusToAttack(abilityScoreBonusToAttack);
            Definition.SetAbilityScore(abilityScore);
            Definition.SetEffectDescription(effectDescription);
            Definition.SetGuiPresentation(guiPresentation);
        }
    }
}
