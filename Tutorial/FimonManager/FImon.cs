using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{
    [BsonIgnoreExtraElements]
    class FImon
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("primary_type")]
        public Type PrimaryType { get; set; }

        [BsonElement("secondary_type")]
        public Type SecondaryType { get; set; }        

        [BsonElement("londec")]
        public double Longtitude { get; set; }

        [BsonElement("strength")]
        public int Strength { get; set; }

    }
}
