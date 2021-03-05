using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{   
    public abstract class Ability
    {
        public Ability(ulong id,AbilityType abilityType, FImonType abilityForm, string name, string description)
        {
            Id = id;
            AbilityType = abilityType;
            AbilityForm = abilityForm;
            Description = description;
            Name = name.Trim();           
        }

        [BsonId]
        public ulong Id { get; set; }

        [BsonElement("ability_type")]
        public AbilityType AbilityType { get; set; }

        [BsonElement("ability_form")]
        public FImonType AbilityForm { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }        
    }
}
