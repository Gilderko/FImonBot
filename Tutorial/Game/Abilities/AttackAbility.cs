using MongoDB.Bson.Serialization.Attributes;
using FImonBotDiscord.Game.FImons;
using FImonBotDiscord.Game.Stats;

namespace FImonBotDiscord.Game.Abilities
{
    public class AttackAbility : Ability
    {
        public AttackAbility(ulong id, AbilityType abilityType, ElementalTypes abilityForm, string name, string description, int cost, int upperDamage, int lowerDamage
            , int hitChance, int critChance) : base(id, abilityType, abilityForm, name, description, cost)
        {
            DamageValueUpper = upperDamage;
            DamageValueLower = lowerDamage;
            HitChance = hitChance;
            CritChance = critChance;
        }

        [BsonElement("damage_value_upper")]
        public int DamageValueUpper { get; private set; }

        [BsonElement("damage_value_lower")]
        public int DamageValueLower { get; private set; }

        [BsonElement("hit_chance")]
        public int HitChance { get; private set; }

        [BsonElement("crit_chance")]
        public int CritChance { get; private set; }

        public string GetDescriptionForMessage()
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization \nDamage range: {DamageValueLower} - {DamageValueUpper}" +
                $"\nEnergy cost: {AbilityCost}\nHit chance: {HitChance}\nCritical hit chance is: {CritChance}";
            return data;
        }

        public string GetDescriptionWithFImon(FImon FImonBase)
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization \nDamage range: {GetLowerDamageWithFImon(FImonBase)} - {GetUpperDamageWithFImon(FImonBase)}" +
                $"\nEnergy cost: {GetCostWithFImon(FImonBase)}  \nHit chance: {GetHitChanceWithFImon(FImonBase)}\nCritical hit chance is: {GetCritChanceWithFImon(FImonBase)}";
            return data;
        }

        public int GetCritChanceWithFImon(FImon FImonBase)
        {
            return CritChance + FImonBase.Luck * BaseStats.luckCritChanceIncrease;
        }
        public int GetHitChanceWithFImon(FImon FImonBase)
        {
            return HitChance + FImonBase.Perception * BaseStats.perceptionHitChanceIncrease;
        }
        public int GetLowerDamageWithFImon(FImon FImonBase)
        {
            if (AbilityType == AbilityType.AutoAttack)
            {
                return (int)(DamageValueLower * (1 + FImonBase.Strength * BaseStats.strengthAutoAttackDamageIncrease / 100f));
            }
            else
            {
                return (int)(DamageValueLower * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerIntensityIncrease / 100f));
            }
        }
        public int GetUpperDamageWithFImon(FImon FImonBase)
        {
            if (AbilityType == AbilityType.AutoAttack)
            {
                return (int)(DamageValueUpper * (1 + FImonBase.Strength * BaseStats.strengthAutoAttackDamageIncrease / 100f));
            }
            else
            {
                return (int)(DamageValueUpper * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerIntensityIncrease / 100f));
            }
        }
    }
}
