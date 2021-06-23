using FImonBot.Game.Abilities;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FImonBot.Game
{
    /// <summary>
    /// Class used for Loading, Adding (Saving), Geting, DefensiveAbilities and AttackAbilities
    /// </summary>
    public static class AbilityManager
    {
        private static ConcurrentDictionary<ulong, AttackAbility> attackAbilities = new ConcurrentDictionary<ulong, AttackAbility>();
        private static ConcurrentDictionary<ulong, DefensiveAbility> defensiveAbilities = new ConcurrentDictionary<ulong, DefensiveAbility>();
        private static IMongoCollection<AttackAbility> attackCollection = null;
        private static IMongoCollection<DefensiveAbility> defensiveCollection = null;
        private const string attackCollectionName = "AttackAbilities";
        private const string defensiveCollectionName = "DefensiveAbilities";

        /// <summary>
        /// Method used for downloading all the abilities from remote MongoDB into cache
        /// </summary>
        public static async Task InitAndLoad()
        {
            if (attackCollection == null || defensiveCollection == null)
            {
                throw new MongoException("database collection not connected");
            }

            List<AttackAbility> attAb = (await attackCollection.FindAsync(s => true)).ToList();
            List<DefensiveAbility> defAb = (await defensiveCollection.FindAsync(s => true)).ToList();

            Parallel.ForEach(attAb, attackAbility => attackAbilities.AddOrUpdate(attackAbility.Id, attackAbility, (ID, ability) => ability));
            Parallel.ForEach(defAb, defensiveAbility => defensiveAbilities.AddOrUpdate(defensiveAbility.Id, defensiveAbility, (ID, ability) => ability));
        }

        /// <summary>
        /// Method used for adding a new ability to database and cache
        /// </summary>
        /// <param name="newAbility"></param>
        public static void AddAbility(Ability newAbility)
        {
            if (attackCollection == null || defensiveCollection == null)
            {
                throw new MongoException("database collection not connected");
            }
            if (attackAbilities.ContainsKey(newAbility.Id) || defensiveAbilities.ContainsKey(newAbility.Id))
            {
                throw new ArgumentException("given ID already exists");
            }

            AttackAbility attackAbility = newAbility as AttackAbility;
            DefensiveAbility defensiveAbility = newAbility as DefensiveAbility;

            if (attackAbility != null)
            {
                attackAbilities.AddOrUpdate(attackAbility.Id, attackAbility, (ID, ability) => ability);
                attackCollection.InsertOne(attackAbility);
            }
            else if (defensiveAbility != null)
            {
                defensiveAbilities.AddOrUpdate(defensiveAbility.Id, defensiveAbility, (ID, ability) => ability);
                defensiveCollection.InsertOne(defensiveAbility);
            }
        }

        internal static void SetCollection(IMongoDatabase database)
        {
            attackCollection = database.GetCollection<AttackAbility>(attackCollectionName);
            defensiveCollection = database.GetCollection<DefensiveAbility>(defensiveCollectionName);
        }

        /// <summary>
        /// Method that returns Defensive or Attack ability as an Ability according to ID
        /// </summary>
        /// <param name="abilityId"></param>
        /// <returns></returns>
        public static Ability GetAbility(ulong? abilityId)
        {
            if (!abilityId.HasValue)
            {
                return null;
            }

            if (attackAbilities.ContainsKey(abilityId.Value))
            {
                return attackAbilities[abilityId.Value];
            }
            else if (defensiveAbilities.ContainsKey(abilityId.Value))
            {
                return defensiveAbilities[abilityId.Value];
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<AttackAbility> GetAttackAbilities()
        {
            foreach (var ability in attackAbilities.Values)
            {
                yield return ability;
            }
        }

        public static IEnumerable<DefensiveAbility> GetDefensiveAbilities()
        {
            foreach (var ability in defensiveAbilities.Values)
            {
                yield return ability;
            }
        }
    }
}
