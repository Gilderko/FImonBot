using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using FImonBotDiscord.Game.Abilities;
using FImonBotDiscord.Game.Stats;

namespace FImonBotDiscord.Game.FImons
{
    [BsonIgnoreExtraElements]
    public class FImon
    {
        public delegate void OnFimonUpdate(FImon thisFimon);
        public event OnFimonUpdate UpdateFImonDatabase;

        public FImon(ulong id,string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower)
        {
            Console.WriteLine("Constructor");
            FImonID = id;
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
        public ulong FImonID { get; private set; }

        [BsonElement("name")]
        public string Name { get; private set; }

        [BsonElement("description")]
        public string Description { get; private set; }

        [BsonElement("primary_type")]
        public ElementalTypes PrimaryType { get; private set; }

        [BsonElement("secondary_type")]
        public ElementalTypes SecondaryType { get; private set; }

        [BsonElement("experience")]
        public int Experience { get; private set; }

        [BsonElement("skill_points_unspent")]
        public int UnspentSkillPoints { get; private set; }

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
        public ulong? AutoAttackID { get; private set; }

        [BsonElement("basic_attack")]
        public ulong? BasicAttackID { get; private set; }

        [BsonElement("special_attack")]
        public ulong? SpecialAttackID { get; private set; }

        [BsonElement("final_attack")]
        public ulong? FinalAttackID { get; private set; }

        [BsonElement("defensive_ability")]
        public ulong? DefensiveAbilityID { get; private set; }

        // This part is not for saving data
       
        [BsonIgnore]
        public AttackAbility AutoAttack { get; private set; }

        [BsonIgnore]
        public AttackAbility BasicAttack { get; private set; }

        [BsonIgnore]
        public AttackAbility SpecialAttack { get; private set; }

        [BsonIgnore]
        public AttackAbility FinalAttack { get; private set; }

        [BsonIgnore]
        public DefensiveAbility DefensiveAbility { get; private set; }

        public int GetLevel()
        {
            return BaseStats.TellLevel(Experience);
        }

        public void SetNewAbility(Ability ability)
        {
            FImon fImon = this;
            InitialiseAbility(ability);
            UpdateFImonDatabase(this);
        }

        public void InitialiseAbility(Ability ability)
        {         
            if (ability == null)
            {
                return;
            }
            if (ability.AbilityType == AbilityType.AutoAttack)
            {
                AutoAttack = (ability as AttackAbility);
                AutoAttackID = ability.Id;
            }
            else if (ability.AbilityType == AbilityType.BasicAttack)
            {
                BasicAttack = (ability as AttackAbility);
                BasicAttackID = ability.Id;
            }
            else if (ability.AbilityType == AbilityType.SpecialAttack)
            {
                SpecialAttack = (ability as AttackAbility);
                SpecialAttackID = ability.Id;
            }
            else if (ability.AbilityType == AbilityType.FinalAttack)
            {
                FinalAttack = (ability as AttackAbility);
                FinalAttackID = ability.Id;
            }
            else if (ability.AbilityType == AbilityType.DefensiveAbility)
            {
                DefensiveAbility = (ability as DefensiveAbility);
                DefensiveAbilityID = ability.Id;
            }
        }

        public int AwardExperience(int experience)
        {
            FImon newFImon = this;
            int currentLevel = BaseStats.TellLevel(newFImon.Experience);

            int modifiedExperience = (int)(experience * (1 + newFImon.Inteligence * BaseStats.inteligenceExpGainIncrease / 100f - newFImon.Luck * BaseStats.luckExpGainDecrease / 100f));
            newFImon.Experience += modifiedExperience;

            int newLevel = BaseStats.TellLevel(newFImon.Experience);
            if (newLevel > currentLevel)
            {
                newFImon.UnspentSkillPoints += BaseStats.abilityPointsToAddOnLevelUp * (newLevel - currentLevel);
            }

            UpdateFImonDatabase(this);
            return modifiedExperience;
        }

        public void IncreaseAttribute(string statType, int statIncrease)
        {
            switch (statType)
            {
                case "strength":
                    Strength += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                case "stamina":
                    Stamina += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                case "ability power":
                    AbilityPower += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                case "inteligence":
                    Inteligence += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                case "luck":
                    Luck += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                case "agility":
                    Agility += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                case "perception":
                    Perception += statIncrease;
                    UnspentSkillPoints -= statIncrease;
                    break;
                default:
                    break;
            }
            UpdateFImonDatabase(this);
        }
    }
}
