﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using FImonBotDiscord.Game.FImons;

namespace FImonBotDiscord.Game.Trainers
{
    [BsonIgnoreExtraElements]
    public class Trainer
    {
        public delegate void OnTrainerUpdate(Trainer thisTrainer);
        public event OnTrainerUpdate UpdateTrainerDatabase;

        public Trainer(ulong ID, string name, string backstory, string imageUrl)
        {
            TrainerID = ID;
            Name = name;
            Backstory = backstory;
            ImageUrl = imageUrl;
            BattlesLost = 0;
            BattlesWon = 0;
        }

        [BsonId]
        public ulong TrainerID { get; private set; }

        [BsonElement("name")]
        public string Name { get; private set; }

        [BsonElement("backstory")]
        public string Backstory { get; private set; }

        [BsonElement("url_to_image")]
        public string ImageUrl { get; private set; }

        [BsonElement("fImon1_ID")]
        public ulong? FImon1ID { get; private set; }

        [BsonElement("fImon2_ID")]
        public ulong? FImon2ID { get; private set; }

        [BsonElement("fImon3_ID")]
        public ulong? FImon3ID { get; private set; }

        [BsonElement("fImon4_ID")]
        public ulong? FImon4ID { get; private set; }

        [BsonElement("battles_win")]
        public ulong BattlesWon { get; set; }

        [BsonElement("battles_lost")]
        public ulong BattlesLost { get; set; }

        [BsonIgnore]
        public FImon FImon1 { get;  set; }

        [BsonIgnore]
        public FImon FImon2 { get;  set; }

        [BsonIgnore]
        public FImon FImon3 { get;  set; }

        [BsonIgnore]
        public FImon FImon4 { get;  set; }

        public void AddFImon(FImon fImonToAdd)
        {
            if (FImon1 == null)
            {
                FImon1 = fImonToAdd;
                FImon1ID = fImonToAdd.FImonID;
            }
            else if (FImon2 == null)
            {
                FImon2 = fImonToAdd;
                FImon2ID = fImonToAdd.FImonID;
            }
            else if (FImon3 == null)
            {
                FImon3 = fImonToAdd;
                FImon3ID = fImonToAdd.FImonID;
            }
            else if (FImon4 == null)
            {
                FImon4 = fImonToAdd;
                FImon4ID = fImonToAdd.FImonID;
            }
            UpdateTrainerDatabase(this);
        }

        public bool CanAddFImon()
        {
            return !(FImon1ID == null && FImon2ID == null && FImon3ID == null && FImon4ID == null);
        }

        public bool HasFImon()
        {
            return FImon1 != null || FImon2 != null || FImon3 != null || FImon4 != null;
        }
    }
}
