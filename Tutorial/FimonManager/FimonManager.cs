using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;


namespace Tutorial.FImons
{
    public static class FImonManager
    {
        public static Dictionary<ulong, FImon> mapping = new Dictionary<ulong, FImon>();
        private static IMongoCollection<FImon> collection = null;
        private const string collectionName = "FImons";

        public static void LoadFimons()
        {
            if (collection == null) { return; }
            Console.WriteLine("Loading Fimons");           

            var allFImon = collection.Find(s => true).ToList();
            foreach (var FImon in allFImon)
            {
                mapping.Add(FImon.DiscordUserID, FImon);
            }
        }

        public static void AddFimon(FImon newFImon)
        {
            if (collection == null) 
            {
                Console.WriteLine("no database");
                return; 
            }

            if (mapping.ContainsKey(newFImon.DiscordUserID))
            {
                Console.WriteLine("already have this ID");
                return;
            }

            Console.WriteLine("Adding FImon");            

            mapping.Add(newFImon.DiscordUserID, newFImon);            
            
            collection.InsertOne(newFImon);
            Console.WriteLine("FImon added");
        }

        internal static void SetCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<FImon>(collectionName);
        }

        public static FImon GetFimon(ulong discordUserID)
        {
            if (!mapping.ContainsKey(discordUserID))
            {
                return null;
            }
            return mapping[discordUserID];
        }

        public static void SetAbility(ulong userID, ulong abilityID)
        {
            Ability ability = AbilityManager.GetAbility(abilityID);
            if (ability.AbilityType == AbilityType.AutoAttack)
            {
                mapping[userID].AutoAttackID = abilityID;
            }
            else if (ability.AbilityType == AbilityType.BasicAttack)
            {
                mapping[userID].BasicAttackID = abilityID;
            }
            else if (ability.AbilityType == AbilityType.SpecialAttack)
            {
                mapping[userID].SpecialAttackID = abilityID;
            }
            else if (ability.AbilityType == AbilityType.UltimateAttack)
            {
                mapping[userID].FinalAttackID = abilityID;
            }
            else if (ability.AbilityType == AbilityType.DefensiveAbility)
            {
                mapping[userID].DefensiveAbilityID = abilityID;
            }
            collection.ReplaceOne(s => s.DiscordUserID == userID, mapping[userID]);
        }        

        public static int AwardExperience(ulong userID,int experience)
        {
            FImon newFImon = mapping[userID];
            int currentLevel = BaseStats.TellLevel(newFImon.Experience);
            
            int modifiedExperience = (int) (experience * (1 +  newFImon.Inteligence * BaseStats.inteligenceExpGainIncrease/100f - newFImon.Luck * BaseStats.luckExpGainDecrease/100f));
            newFImon.Experience += modifiedExperience;

            int newLevel = BaseStats.TellLevel(newFImon.Experience);
            if (newLevel > currentLevel)
            {
                newFImon.UnspentSkillPoints += BaseStats.abilityPointsToAddOnLevelUp;
            }

            collection.ReplaceOne(s => s.DiscordUserID == userID, mapping[userID]);
            return modifiedExperience;
        }
    }
}
