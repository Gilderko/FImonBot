using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Tutorial.Game.Trainers;


namespace Tutorial.Game
{
    static class TrainerManager
    {
        private static Dictionary<ulong, Trainer> trainerMapping = new Dictionary<ulong, Trainer>();
        private static IMongoCollection<Trainer> collection = null;
        private const string collectionName = "Trainers";
        
        public static void LoadTrainers()
        {
            if (collection == null) { return; }
            Console.WriteLine("Loading Trainers");

            var allTrainers = collection.Find(s => true).ToList();

            foreach (var currentTrainer in allTrainers)
            {
                trainerMapping.Add(currentTrainer.TrainerID, currentTrainer);
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

            trainerMapping.Add(newTrainer.TrainerID, newTrainer);
            newTrainer.UpdateTrainerDatabase += UpdateTrainer;

            collection.InsertOne(newTrainer);
            Console.WriteLine("Trainer added");
        }

        public static void UpdateTrainer(Trainer trainerToUpdate)
        {
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

        internal static void SetCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<Trainer>(collectionName);
        }
    }
}
