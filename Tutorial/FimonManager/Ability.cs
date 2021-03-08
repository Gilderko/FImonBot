using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FImons
{   
    public abstract class Ability
    {
        public Ability(ulong id,AbilityType abilityType, ElementalTypes abilityForm, string name, string description, int abilityCost)
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
        public ulong Id { get; set; }

        [BsonElement("ability_type")]
        public AbilityType AbilityType { get; set; }

        [BsonElement("elemental_type")]
        public ElementalTypes ElementalType { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }     
        
        [BsonElement("ability_cost")]
        public int AbilityCost { get; set; }

        public int GetCostWithFImon(InCombatFImon FImonBase)
        {
            int cost = AbilityCost;
            if (AbilityType == AbilityType.AutoAttack)
            {
                cost = (int)(cost * (1 + FImonBase.FImonBase.Strength * BaseStats.strengthAutoAttackCostIncrease / 100f));

            }
            else
            {
                cost = (int)(cost * (1 + FImonBase.FImonBase.AbilityPower * BaseStats.abilityPowerCostIncrease / 100f));
            }
            return cost;
        }
    }
}
