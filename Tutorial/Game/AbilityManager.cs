using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Tutorial.Game.Abilities;

namespace Tutorial.Game

{
    public static class AbilityManager
    {
        private static Dictionary<ulong, AttackAbility> attackAbilities = new Dictionary<ulong, AttackAbility>();
        private static Dictionary<ulong, DefensiveAbility> defensiveAbilities = new Dictionary<ulong, DefensiveAbility>();
        private static IMongoCollection<AttackAbility> attackCollection = null;
        private static IMongoCollection<DefensiveAbility> defensiveCollection = null;
        private const string attackCollectionName = "AttackAbilities";
        private const string defensiveCollectionName = "DefensiveAbilities";

        public static void LoadAbilities()
        {
            if (attackCollection == null || defensiveCollection == null) { return; }
            Console.WriteLine("Loading Abilities");

            var attAb = attackCollection.Find(s => true).ToList();
            var defAb = defensiveCollection.Find(s => true).ToList();
            foreach (var attackAbility in attAb)
            {
                attackAbilities.Add(attackAbility.Id, attackAbility);
            }
            foreach (var defensiveAbility in defAb)
            {
                defensiveAbilities.Add(defensiveAbility.Id, defensiveAbility);
            }
        }

        public static void AddAbility(Ability newAbility)
        {
            if (attackCollection == null || defensiveCollection == null) { return; }

            if (attackAbilities.ContainsKey(newAbility.Id) || defensiveAbilities.ContainsKey(newAbility.Id))
            {
                Console.WriteLine("already have this ID");
                return;
            }
            Console.WriteLine("Adding Ability");

            AttackAbility attackAbility = newAbility as AttackAbility;
            DefensiveAbility defensiveAbility = newAbility as DefensiveAbility;

            if (attackAbility != null)
            {
                Console.WriteLine("Adding Attack");
                attackAbilities.Add(attackAbility.Id, attackAbility);
                attackCollection.InsertOne(attackAbility);
            }
            else if (defensiveAbility != null)
            {
                Console.WriteLine("Adding Defense");
                defensiveAbilities.Add(defensiveAbility.Id, defensiveAbility);
                defensiveCollection.InsertOne(defensiveAbility);
            }
            Console.WriteLine("Ability added");
        }
        public static void SetCollection(IMongoDatabase database)
        {
            attackCollection = database.GetCollection<AttackAbility>(attackCollectionName);
            defensiveCollection = database.GetCollection<DefensiveAbility>(defensiveCollectionName);
        }

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
