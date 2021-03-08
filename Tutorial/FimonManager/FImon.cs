using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FImons
{
    [BsonIgnoreExtraElements]
    public class FImon
    {
        public FImon (ulong id, string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower)
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
            AbilityPower = abilityPower;
            AutoAttackID = null;
            BasicAttackID = null;
            SpecialAttackID = null;
            FinalAttackID = null;
            Experience = 0;
            UnspentSkillPoints = 0;
        }

        [BsonId]
        public ulong DiscordUserID { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("primary_type")]
        public ElementalTypes PrimaryType { get; set; }

        [BsonElement("secondary_type")]
        public ElementalTypes SecondaryType { get; set; }

        [BsonElement("experience")]
        public int Experience { get; set; }

        [BsonElement("skill_points_unspent")]
        public int UnspentSkillPoints { get; set; }

        [BsonElement("strength")]
        public int Strength { get; set; }

        [BsonElement("stamina")]
        public int Stamina { get; set; }

        [BsonElement("ability_power")]
        public int AbilityPower { get; set; }

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
        public ulong? FinalAttackID { get; set; }

        [BsonElement("defensive_ability")]
        public ulong? DefensiveAbilityID { get;set; }

        
    }
}
