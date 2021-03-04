using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;


namespace Tutorial.FimonManager
{
    static class FimonManager
    {
        public static Dictionary<ulong, string> mapping = new Dictionary<ulong, string>();

        internal static void LoadFimons(IMongoDatabase database)
        {
            var collection = database.GetCollection<BsonDocument>("FImons");
        }
    }
}
