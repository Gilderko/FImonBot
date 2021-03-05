using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{
    [BsonIgnoreExtraElements]
    public class Ability
    {
        public Ability(AbilityType abilityType, FImonType attackType, string description, string name)
        {
            AbilityType = abilityType;
            AttackType = attackType;
            Description = description;
            Name = name;
        }

        [BsonId]
        public AbilityType AbilityType { get; set; }

        [BsonElement("attack_type")]
        public FImonType AttackType { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }        
    }
}
