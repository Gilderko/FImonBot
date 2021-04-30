using MongoDB.Bson.Serialization.Attributes;
using FImonBot.Game.FImons;
using FImonBot.Game.Stats;

namespace FImonBot.Game.Abilities
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
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization" +
                $"\nHeals for: {HealValue} | Energy cost: {AbilityCost} | Charges: {Charges}";
            return data;
        }

        public string GetDescriptionWithFImon(FImon FImonBase)
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization\n" +
                $"Heals for: {GetHealValueWithFImon(FImonBase)} | Energy cost: {GetCostWithFImon(FImonBase)} | Charges: {Charges}";
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
