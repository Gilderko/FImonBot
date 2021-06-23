
using FImonBot.Game.FImons;
using FImonBot.Game.Trainers;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FImonBot.Game.Trainers.Trainer;

namespace FImonBot.Game
{
    /// <summary>
    /// Class used for Loading, Adding (Saving), Geting, Deleting Trainers and Trainers FImons
    /// </summary>
    public static class TrainerManager
    {
        private static ConcurrentDictionary<ulong, Trainer> trainerMapping = new ConcurrentDictionary<ulong, Trainer>();
        private static IMongoCollection<Trainer> collection = null;
        private const string collectionName = "Trainers";

        public delegate void FImonIDDelegate(ulong fimonID);
        public static event FImonIDDelegate TrainersFImonDeleted;

        public static event CreatingFImonDelegate CreateFImon;


        /// <summary>
        /// Method used for downloading all the Trainerns from remote MongoDB into cache and setting their FImons
        /// </summary>
        public static async Task InitAndLoad()
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            List<Trainer> allTrainers = (await collection.FindAsync(s => true)).ToList();
            Parallel.ForEach(allTrainers, currentTrainer =>
            {
                trainerMapping.AddOrUpdate(currentTrainer.TrainerID, currentTrainer, ((ID, Trainer) => Trainer));
                currentTrainer.FImon1 = !currentTrainer.FImon1ID.HasValue ? null : FImonManager.GetFimon(currentTrainer.FImon1ID.Value);
                currentTrainer.FImon2 = !currentTrainer.FImon2ID.HasValue ? null : FImonManager.GetFimon(currentTrainer.FImon2ID.Value);
                currentTrainer.FImon3 = !currentTrainer.FImon3ID.HasValue ? null : FImonManager.GetFimon(currentTrainer.FImon3ID.Value);
                currentTrainer.FImon4 = !currentTrainer.FImon4ID.HasValue ? null : FImonManager.GetFimon(currentTrainer.FImon4ID.Value);
                currentTrainer.UpdateTrainerDatabase += UpdateTrainer;
                currentTrainer.DeleteTrainersFImon += DeleteTrainersFImon;
                currentTrainer.CreateFImonEvent += CreateTrainersFImon;
            });
        }

        /// <summary>
        /// Method used for creating and adding Trainers into cache and remote MongoDB
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="name"></param>
        /// <param name="backstory"></param>
        /// <param name="imageUrl"></param>
        public static void AddTrainer(ulong ID, string name, string backstory, string imageUrl)
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            if (trainerMapping.ContainsKey(ID))
            {
                throw new ArgumentException("given ID already exists");
            }

            var newTrainer = new Trainer(ID, name, backstory, imageUrl);

            trainerMapping.AddOrUpdate(newTrainer.TrainerID, newTrainer, ((ID, Trainer) => Trainer));
            newTrainer.UpdateTrainerDatabase += UpdateTrainer;
            newTrainer.DeleteTrainersFImon += DeleteTrainersFImon;
            newTrainer.CreateFImonEvent += CreateTrainersFImon;

            collection.InsertOne(newTrainer);
        }

        /// <summary>
        /// Method used for when Trainer updates its values so that the updates are propagated into remote MongoDB
        /// </summary>
        /// <param name="trainerToUpdate"></param>
        public static void UpdateTrainer(Trainer trainerToUpdate)
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            collection.ReplaceOne(s => s.TrainerID == trainerToUpdate.TrainerID, trainerToUpdate);
        }

        /// <summary>
        /// Method used for geting Trainer based on DiscordUsersID
        /// </summary>
        /// <param name="trainerID"></param>
        /// <returns></returns>
        public static Trainer GetTrainer(ulong trainerID)
        {
            if (!trainerMapping.ContainsKey(trainerID))
            {
                return null;
            }

            return trainerMapping[trainerID];
        }

        /// <summary>
        /// Method used for deleting Trainers! Deleting Trainer also results in deleting his FImons!
        /// </summary>
        /// <param name="trainerID"></param>
        public static void DeleteTrainer(ulong trainerID)
        {
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            if (!trainerMapping.ContainsKey(trainerID))
            {
                throw new ArgumentException("given trainer ID doesnt exist");
            }

            Trainer trainer = trainerMapping[trainerID];

            if (trainer.FImon1ID.HasValue)
            {
                TrainersFImonDeleted(trainer.FImon1ID.Value);
            }
            if (trainer.FImon2ID.HasValue)
            {
                TrainersFImonDeleted(trainer.FImon2ID.Value);
            }
            if (trainer.FImon3ID.HasValue)
            {
                TrainersFImonDeleted(trainer.FImon3ID.Value);
            }
            if (trainer.FImon4ID.HasValue)
            {
                TrainersFImonDeleted(trainer.FImon4ID.Value);
            }

            trainerMapping.Remove(trainerID, out trainer);
            collection.DeleteOne(trainer => trainer.TrainerID == trainerID);
        }

        /// <summary>
        /// Method used for deleting FImon of a Trainer (also deletes FImon from the cache and remote MongoDB)
        /// </summary>
        /// <param name="trainerId"></param>
        /// <param name="FImonId"></param>
        private static void DeleteTrainersFImon(ulong FImonId)
        {
            Console.WriteLine("FIMON DELETE IN TRAINER MANAGER");
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }
            TrainersFImonDeleted(FImonId);
        }

        private static FImon CreateTrainersFImon(string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower)
        {
            Console.WriteLine("New FIMON creation in TRAINERMANAGER");
            if (collection == null)
            {
                throw new MongoException("database collection not connected");
            }

            return CreateFImon(name, desc, primaryType, secondaryType, strength, stamina,
             inteligence, luck, agility, perception, abilityPower);
        }

        internal static void SetCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<Trainer>(collectionName);
        }
    }
}
