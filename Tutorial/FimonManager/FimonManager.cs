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

        
    }
}
