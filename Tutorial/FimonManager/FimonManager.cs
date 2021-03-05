using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;


namespace Tutorial.FimonManager
{
    public static class FimonManager
    {
        public static Dictionary<ulong, FImon> mapping = new Dictionary<ulong, FImon>();
        private static IMongoDatabase mongoDatabase = null;

        public static void LoadFimons()
        {
            if (mongoDatabase == null) { return; }
            Console.WriteLine("Loading Fimons");
            var collection = mongoDatabase.GetCollection<FImon>("FImons");

            var allFImon = collection.Find(s => true).ToList();
            foreach (var FImon in allFImon)
            {
                mapping.Add(FImon.DiscordUserID, FImon);
            }
        }

        public static void AddFimon(FImon newFImon)
        {
            if (mongoDatabase == null) 
            {
                Console.WriteLine("no database");
                return; 
            }
            Console.WriteLine("Adding FImon");
            var collection = mongoDatabase.GetCollection<FImon>("FImons");

            mapping.Add(newFImon.DiscordUserID, newFImon);            
            
            collection.InsertOne(newFImon);
            Console.WriteLine("FImon added");
        }

        internal static void SetDatabase(MongoClient client)
        {
            mongoDatabase = client.GetDatabase("FImonDB");
        }

        public static FImon GetFimon(ulong discordUserID)
        {
            if (!mapping.ContainsKey(discordUserID))
            {
                return null;
            }
            return mapping[discordUserID];
        }

        public static void AddFimonAbility(ulong discordUserID, Ability attack)
        {
            AbilityType whatAbility = attack.AbilityType;
            FImon FImon = GetFimon(discordUserID);
            switch (whatAbility)
            {
                case AbilityType.BasicAttack:
                    FImon.BasicAttack = attack;
                    break;
                case AbilityType.SpecialAbility1:
                    FImon.SpecialAttack1 = attack;
                    break;
                case AbilityType.SpecialAbility2:
                    FImon.SpecialAttack2 = attack;
                    break;
                case AbilityType.SpecialAbility3:
                    FImon.SpecialAttack3 = attack;
                    break;
                case AbilityType.DefensiveAbility:
                    FImon.DefensiveAbility = (DefensiveAbility) attack;
                    break;
            }
            var collection = mongoDatabase.GetCollection<FImon>("FImons");
            collection.FindOneAndReplace((s => s.DiscordUserID == discordUserID), FImon);
        }
    }
}
