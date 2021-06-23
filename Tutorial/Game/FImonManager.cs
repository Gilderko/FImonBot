using FImonBot.Game.FImons;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FImonBot.Game
{
    /// <summary>
    /// Class used for Loading, Adding (Saving), Geting, Deleting FImons
    /// </summary>
    public static class FImonManager
    {
        private static ConcurrentDictionary<ulong, FImon> mapping = new ConcurrentDictionary<ulong, FImon>();
        private static IMongoCollection<FImon> collection = null;
        private const string collectionName = "FImons";
        private static ulong newIDToAllocate { get; set; } = 1;

        /// <summary>
        /// Method used for downloading all the FImons from remote MongoDB into cache and setting their Abilities
        /// </summary>
        public static async Task InitAndLoad()
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            var allFImon = (await collection.FindAsync(s => true)).ToList();

            Parallel.ForEach(allFImon, currentFImon =>
            {
                mapping.AddOrUpdate(currentFImon.FImonID, currentFImon, (ID, fimon) => fimon);
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
            });
            TrainerManager.TrainersFImonDeleted += DeleteFImon;
            TrainerManager.CreateFImon += AddFimon;
            newIDToAllocate += 1;
        }

        /// <summary>
        /// Method used for creating and adding new FImon into the cache and remote MongoDB
        /// </summary>
        /// <param name="trainerID"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <param name="primaryType"></param>
        /// <param name="secondaryType"></param>
        /// <param name="strength"></param>
        /// <param name="stamina"></param>
        /// <param name="inteligence"></param>
        /// <param name="luck"></param>
        /// <param name="agility"></param>
        /// <param name="perception"></param>
        /// <param name="abilityPower"></param>
        private static FImon AddFimon(string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower)
        {
            Console.WriteLine("New FIMON creation in FIMONMANAGER");
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            if (mapping.ContainsKey(newIDToAllocate))
            {
                throw new ArgumentException("given fimon ID already exists");
            }

            var newFImon = new FImon(newIDToAllocate, name, desc, primaryType, secondaryType, strength, stamina, inteligence, luck, agility, perception, abilityPower);

            newIDToAllocate += 1;

            mapping.AddOrUpdate(newFImon.FImonID, newFImon, (ID, fimon) => fimon);
            newFImon.UpdateFImonDatabase += UpdateFImon;

            collection.InsertOne(newFImon);
            return newFImon;
        }

        internal static void SetCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<FImon>(collectionName);
        }

        /// <summary>
        /// Method used for geting FImon according to his ID
        /// </summary>
        /// <param name="FImonID"></param>
        /// <returns></returns>
        public static FImon GetFimon(ulong FImonID)
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            if (!mapping.ContainsKey(FImonID))
            {
                return null;
            }

            return mapping[FImonID];
        }

        /// <summary>
        /// Method used for when FImon updates its values so that the updates are propagated into remote MongoDB
        /// </summary>
        /// <param name="fImon"></param>
        private static void UpdateFImon(FImon fImon)
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            collection.ReplaceOne(s => s.FImonID == fImon.FImonID, fImon);
        }

        /// <summary>
        /// Method used for deleting FImon from cache and MongoDB but not from Trainer! To delete FImon of a Trainer
        /// use Trainer
        /// </summary>
        /// <param name="fImonID"></param>
        private static void DeleteFImon(ulong fImonID)
        {
            Console.WriteLine("FIMON DELETE IN FIMON MANAGER");
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            if (!mapping.ContainsKey(fImonID))
            {
                throw new ArgumentException("given fimon ID doesnt exist");
            }

            FImon result;
            mapping.Remove(fImonID, out result);
            collection.DeleteOne(FImon => FImon.FImonID == fImonID);
        }
    }
}
