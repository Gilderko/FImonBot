using FImonBot.Game.FImons;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FImonBot.Game.Trainers
{
    [BsonIgnoreExtraElements]
    public class Trainer
    {
        public delegate void OnTrainerUpdate(Trainer thisTrainer);
        public event OnTrainerUpdate UpdateTrainerDatabase;

        public delegate void OnTrainerFImonID(ulong FImonID);
        public event OnTrainerFImonID DeleteTrainersFImon;

        public delegate FImon CreatingFImonDelegate(string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower);
        public event CreatingFImonDelegate CreateFImonEvent;

        public Trainer(ulong ID, string name, string backstory, string imageUrl)
        {
            TrainerID = ID;
            Name = name;
            Backstory = backstory;
            ImageUrl = imageUrl;
            BattlesLost = 0;
            BattlesWon = 0;
            Experience = 0;
            FImon1ID = null;
            FImon2ID = null;
            FImon3ID = null;
            FImon4ID = null;
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
        public ulong BattlesWon { get; private set; }

        [BsonElement("battles_lost")]
        public ulong BattlesLost { get; private set; }

        [BsonElement("experience")]
        public int Experience { get; private set; }

        [BsonIgnore]
        public FImon FImon1 { get; set; }

        [BsonIgnore]
        public FImon FImon2 { get; set; }

        [BsonIgnore]
        public FImon FImon3 { get; set; }

        [BsonIgnore]
        public FImon FImon4 { get; set; }

        public void AddFImon(string name, string desc, ElementalTypes primaryType, ElementalTypes secondaryType, int strength, int stamina,
            int inteligence, int luck, int agility, int perception, int abilityPower)
        {
            Console.WriteLine("New FIMON creation in TRAINER");
            var fImonToAdd = CreateFImonEvent(name, desc, primaryType, secondaryType, strength, stamina,
             inteligence, luck, agility, perception, abilityPower);

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
            return FImon1ID == null || FImon2ID == null || FImon3ID == null || FImon4ID == null;
        }
        public bool HasFImon()
        {
            return FImon1ID != null || FImon2ID != null || FImon3ID != null || FImon4ID != null;
        }

        public void RemoveFImon(ulong fimonID)
        {
            Console.WriteLine("FIMON DELETE IN TRAINER");
            if (FImon1ID == fimonID)
            {
                Console.WriteLine("DEL 1");
                DeleteTrainersFImon(FImon1ID.Value);
                FImon1ID = null;
                FImon1 = null;
            }
            if (FImon2ID == fimonID)
            {
                Console.WriteLine("DEL 2");
                DeleteTrainersFImon(FImon2ID.Value);
                FImon2ID = null;
                FImon2 = null;
            }
            if (FImon3ID == fimonID)
            {
                Console.WriteLine("DEL 3");
                DeleteTrainersFImon(FImon3ID.Value);
                FImon3ID = null;
                FImon3 = null;
            }
            if (FImon4ID == fimonID)
            {
                Console.WriteLine("DEL 4");
                DeleteTrainersFImon(FImon4ID.Value);
                FImon4ID = null;
                FImon4 = null;
            }
            Console.WriteLine("UPDATE DATABASE");
            UpdateTrainerDatabase(this);
        }

        public void AddExperience(int ammountToAdd)
        {
            Experience += ammountToAdd;
            UpdateTrainerDatabase(this);
        }

        public void RemoveExperience(int ammountToRemove)
        {
            Experience -= ammountToRemove;
            UpdateTrainerDatabase(this);
        }

        public void WonBattle()
        {
            BattlesWon++;
            UpdateTrainerDatabase(this);
        }

        public void LostBattle()
        {
            BattlesLost++;
            UpdateTrainerDatabase(this);
        }
    }
}
