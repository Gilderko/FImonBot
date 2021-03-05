using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{
    public class DefensiveAbility : Ability
    {
        public DefensiveAbility(AbilityType abilityType, FImonType attackType, string description, string name, DefensiveAbilityType defensiveAbilityType) : base(abilityType,attackType,description,name)
        {
            DefensiveAbilityType = defensiveAbilityType;
        }

        [BsonElement("defensive_ability_type")]
        public DefensiveAbilityType DefensiveAbilityType { get; set; }
    }
}
