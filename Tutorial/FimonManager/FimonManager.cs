using MongoDB.Driver;
using System;
using System.Collections.Generic;


namespace Tutorial.FImons
{
    public static class FImonManager
    {
        private static Dictionary<ulong, FImon> mapping = new Dictionary<ulong, FImon>();
        private static IMongoCollection<FImon> collection = null;
        private const string collectionName = "FImons";

        public static void LoadFimons()
        {
            if (collection == null) { return; }
            Console.WriteLine("Loading Fimons");

            var somet = collection.WithReadConcern(new ReadConcern());
            var allFImon = collection.Find(s => true).ToList();
            
            foreach (var currentFImon in allFImon)
            {
                mapping.Add(currentFImon.DiscordUserID, currentFImon);
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.AutoAttackID.Value));               
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.BasicAttackID.Value));
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.SpecialAttackID.Value));
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.FinalAttackID.Value));
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.DefensiveAbilityID.Value));
                currentFImon.UpdateFImonDatabase += UpdateFImon;
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
            newFImon.UpdateFImonDatabase += UpdateFImon;

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

        public static void UpdateFImon(FImon fImon)
        {
            collection.ReplaceOne(s => s.DiscordUserID == fImon.DiscordUserID, fImon);
        }
    }
}
