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
        public DefensiveAbility(ulong id, AbilityType abilityType, FImonType abilityForm, string name, string description, int? healVal = null, int? dodgeVal = null) : base(id,abilityType,abilityForm,name,description)
        {
            HealValue = healVal;
            DodgeValue = dodgeVal;
        }

        [BsonElement("heal_value")]
        public int? HealValue { get; set; }

        [BsonElement("dodge_value")]
        public int? DodgeValue { get; set; }

        public string GetDescriptionForMessage()
        {
            string data = $"{Description}\nThe ability is of {AbilityForm.ToString()} specialization";
            data += HealValue == null ? $"Increases dodge chance by {DodgeValue} for next round" : $"Heals for: {HealValue}";
            return data;
        }
    }
}
