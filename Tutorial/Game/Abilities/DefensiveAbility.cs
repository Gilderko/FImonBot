using MongoDB.Bson.Serialization.Attributes;
using Tutorial.Game.FImons;
using Tutorial.Game.Stats;

namespace Tutorial.Game.Abilities
{
    public class DefensiveAbility : Ability
    {
        public DefensiveAbility(ulong id, AbilityType abilityType, ElementalTypes abilityForm, string name, string description, int cost, int charges, int? healVal = null) : base(id, abilityType, abilityForm, name, description, cost)
        {
            HealValue = healVal;
            Charges = charges;
        }

        [BsonElement("heal_value")]
        public int? HealValue { get; private set; }

        [BsonElement("charges")]
        public int Charges { get; private set; }

        public string GetDescriptionForMessage()
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization\nHeals for: {HealValue}\n Energy cost: {AbilityCost} \nCharges: {Charges}";
            return data;
        }

        public string GetDescriptionWithFImon(FImon FImonBase)
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization\nHeals for: {GetHealValueWithFImon(FImonBase)}\n Energy cost: {GetCostWithFImon(FImonBase)} \nCharges: {Charges}";
            return data;
        }

        public int GetHealValueWithFImon(FImon FImonBase)
        {
            if (HealValue != null)
            {
                return (int)(HealValue * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerIntensityIncrease / 100f));
            }
            return 0;
        }
    }
}
