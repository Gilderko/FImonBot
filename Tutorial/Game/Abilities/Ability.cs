using MongoDB.Bson.Serialization.Attributes;
using System;
using Tutorial.Game.FImons;
using Tutorial.Game.Stats;

namespace Tutorial.Game.Abilities
{
    public abstract class Ability
    {
        public Ability(ulong id, AbilityType abilityType, ElementalTypes abilityForm, string name, string description, int abilityCost)
        {
            if (id == 0)
            {
                throw new Exception();
            }
            Id = id;
            AbilityType = abilityType;
            ElementalType = abilityForm;
            Description = description;
            Name = name.Trim();
            AbilityCost = abilityCost;
        }

        [BsonId]
        public ulong Id { get; private set; }

        [BsonElement("ability_type")]
        public AbilityType AbilityType { get; private set; }

        [BsonElement("elemental_type")]
        public ElementalTypes ElementalType { get; private set; }

        [BsonElement("name")]
        public string Name { get; private set; }

        [BsonElement("description")]
        public string Description { get; private set; }

        [BsonElement("ability_cost")]
        public int AbilityCost { get; private set; }

        public int GetCostWithFImon(FImon FImonBase)
        {
            int cost = AbilityCost;
            if (AbilityType == AbilityType.AutoAttack)
            {
                cost = (int)(cost * (1 + FImonBase.Strength * BaseStats.strengthAutoAttackCostIncrease / 100f));

            }
            else
            {
                cost = (int)(cost * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerCostIncrease / 100f));
            }
            return cost;
        }
    }
}
