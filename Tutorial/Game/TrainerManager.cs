using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FImonBotDiscord.Game.Trainers;


namespace FImonBotDiscord.Game
{
    static class TrainerManager
    {
        private static ConcurrentDictionary<ulong, Trainer> trainerMapping = new ConcurrentDictionary<ulong, Trainer>();
        private static IMongoCollection<Trainer> collection = null;
        private const string collectionName = "Trainers";
        
        public static void LoadTrainers()
        {
            if (collection == null) { return; }
            Console.WriteLine("Loading Trainers");

            var allTrainers = collection.Find(s => true).ToList();

            foreach (var currentTrainer in allTrainers)
            {
                trainerMapping.AddOrUpdate(currentTrainer.TrainerID, currentTrainer, ((ID,Trainer) => Trainer));
                currentTrainer.FImon1 = FImonManager.GetFimon(currentTrainer.FImon1ID.GetValueOrDefault());
                currentTrainer.FImon2 = FImonManager.GetFimon(currentTrainer.FImon2ID.GetValueOrDefault());
                currentTrainer.FImon3 = FImonManager.GetFimon(currentTrainer.FImon3ID.GetValueOrDefault());
                currentTrainer.FImon4 = FImonManager.GetFimon(currentTrainer.FImon4ID.GetValueOrDefault());
                currentTrainer.UpdateTrainerDatabase += UpdateTrainer;
            }
        }

        public static void AddTrainer(ulong ID, string name, string backstory, string imageUrl)
        {
            if (collection == null) { return; }

            if (trainerMapping.ContainsKey(ID))
            {
                Console.WriteLine("already have this ID");
                return;
            }

            var newTrainer = new Trainer(ID, name, backstory, imageUrl);

            trainerMapping.AddOrUpdate(newTrainer.TrainerID, newTrainer, ((ID, Trainer) => Trainer));
            newTrainer.UpdateTrainerDatabase += UpdateTrainer;

            collection.InsertOne(newTrainer);
            Console.WriteLine("Trainer added");
        }

        public static void UpdateTrainer(Trainer trainerToUpdate)
        {
            if (collection == null)
            {
                Console.WriteLine("No database");
                return;
            }
            collection.ReplaceOne(s => s.TrainerID == trainerToUpdate.TrainerID, trainerToUpdate);
        }

        public static Trainer GetTrainer(ulong trainerID)
        {
            if (!trainerMapping.ContainsKey(trainerID))
            {
                return null;
            }
            return trainerMapping[trainerID];
        }

        public static void DeleteTrainer(ulong trainerID)
        {
            if (collection == null)
            {
                Console.WriteLine("No database");
                return;
            }
            if (!trainerMapping.ContainsKey(trainerID))
            {
                return;
            }

            var trainer = trainerMapping[trainerID];

            if (trainer.FImon1ID.HasValue) { FImonManager.DeleteFImon(trainer.FImon1ID.Value); }
            if (trainer.FImon2ID.HasValue) { FImonManager.DeleteFImon(trainer.FImon2ID.Value); }
            if (trainer.FImon3ID.HasValue) { FImonManager.DeleteFImon(trainer.FImon3ID.Value); }
            if (trainer.FImon4ID.HasValue) { FImonManager.DeleteFImon(trainer.FImon4ID.Value); }

            trainerMapping.Remove(trainerID, out trainer);
            collection.DeleteOne(trainer => trainer.TrainerID == trainerID);
        }

        public static void DeleteTrainersFImon(ulong trainerId, ulong FImonId)
        {
            if (collection == null) { return; }
            if (!trainerMapping.ContainsKey(trainerId)) { return; }

            var trainer = trainerMapping[trainerId];
            trainer.RemoveFImon(FImonId);
            FImonManager.DeleteFImon(FImonId);
        }

        internal static void SetCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<Trainer>(collectionName);
        }
    }
}
