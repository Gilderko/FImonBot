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
            BasicAttack = null;
            SpecialAttack1 = null;
            SpecialAttack2 = null;
            SpecialAttack3 = null;
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
        
        [BsonElement("basic_attack")]
        public Ability BasicAttack { get; set; }

        [BsonElement("special_attack_1")]
        public Ability SpecialAttack1 { get; set; }

        [BsonElement("special_attack_2")]
        public Ability SpecialAttack2 { get; set; }

        [BsonElement("special_attack_3")]
        public Ability SpecialAttack3 { get; set; }

        [BsonElement("defensive_ability")]
        public DefensiveAbility DefensiveAbility { get; set; }

    }
}
