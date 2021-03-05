using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{
    [BsonIgnoreExtraElements]
    public class FImon
    {
        public FImon (ulong id, string name, string desc, FImonType primaryType, FImonType secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception)
        {
            DiscordUserID = id;
            Name = name;
            Description = desc;
            PrimaryType = primaryType;
            SecondaryType = secondaryType;
            Strength = strength;
            Stamina = stamina;
            Inteligence = inteligence;
            Luck = luck;
            Agility = agility;
            Perception = perception;
            AutoAttackID = null;
            BasicAttackID = null;
            SpecialAttackID = null;
            FinalAttack = null;
        }

        [BsonId]
        public ulong DiscordUserID { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("primary_type")]
        public FImonType PrimaryType { get; set; }

        [BsonElement("secondary_type")]
        public FImonType SecondaryType { get; set; }        

        [BsonElement("strength")]
        public int Strength { get; set; }

        [BsonElement("stamina")]
        public int Stamina { get; set; }

        [BsonElement("inteligence")]
        public int Inteligence { get; set; }

        [BsonElement("luck")]
        public int Luck { get; set; }

        [BsonElement("agility")]
        public int Agility { get; set; }

        [BsonElement("perception")]
        public int Perception { get; set; }
        
        [BsonElement("auto_attack")]
        public ulong? AutoAttackID { get; set; }

        [BsonElement("basic_attack")]
        public ulong? BasicAttackID { get; set; }

        [BsonElement("special_attack")]
        public ulong? SpecialAttackID { get; set; }

        [BsonElement("final_attack")]
        public ulong? FinalAttack { get; set; }

        [BsonElement("defensive_ability")]
        public ulong? DefensiveAbilityID { get;set; }
    }
}
