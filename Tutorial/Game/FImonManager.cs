using MongoDB.Driver;
using System;
using System.Collections.Generic;
using Tutorial.Game.FImons;

namespace Tutorial.Game
{
    public static class FImonManager
    {
        private static Dictionary<ulong, FImon> mapping = new Dictionary<ulong, FImon>();
        private static IMongoCollection<FImon> collection = null;
        private const string collectionName = "FImons";
        private static ulong newIDToAllocate { get; set; } = 1;

        public static void LoadFimons()
        {
            if (collection == null) { return; }
            Console.WriteLine("Loading Fimons");

            var allFImon = collection.Find(s => true).ToList();
            
            foreach (var currentFImon in allFImon)
            {
                mapping.Add(currentFImon.FImonID, currentFImon);
                if (currentFImon.FImonID > newIDToAllocate)
                {
                    newIDToAllocate = currentFImon.FImonID;
                }
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.AutoAttackID));               
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.BasicAttackID));
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.SpecialAttackID));
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.FinalAttackID));
                currentFImon.InitialiseAbility(AbilityManager.GetAbility(currentFImon.DefensiveAbilityID));
                currentFImon.UpdateFImonDatabase += UpdateFImon;
            }
            newIDToAllocate += 1;
            Console.WriteLine(newIDToAllocate);
        }

        public static void AddFimon(ulong trainerID,string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower)
        { 
            if (collection == null)
            {
                Console.WriteLine("no database");
                return;
            }

            if (mapping.ContainsKey(newIDToAllocate))
            {
                Console.WriteLine("already have this ID");
                return;
            }

            var newFImon = new FImon(newIDToAllocate, name, desc, primaryType, secondaryType, strength, stamina, inteligence, luck, agility, perception, abilityPower);
            var trainer = TrainerManager.GetTrainer(trainerID);
            trainer.AddFImon(newFImon);
            newIDToAllocate += 1;

            Console.WriteLine("Adding FImon");

            mapping.Add(newFImon.FImonID, newFImon);
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
            collection.ReplaceOne(s => s.FImonID == fImon.FImonID, fImon);
        }
    }
}
