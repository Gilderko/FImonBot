using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{
    public static class AbilityManager
    {
        public static Dictionary<ulong, Ability> abilities = new Dictionary<ulong, Ability>();        
        private static IMongoCollection<Ability> collection = null;
        private const string collectionName = "Abilities";
        

        public static void LoadAbilities()
        {
            if (collection == null) { return; }
            Console.WriteLine("Loading Abilities");            

            var allAbilities = collection.Find(s => true).ToList();
            foreach (var ability in allAbilities)
            {
                if (ability.AbilityType == AbilityType.DefensiveAbility)
                {
                    DefensiveAbility defensiveAbility = (DefensiveAbility)ability;
                    abilities.Add(defensiveAbility.Id, defensiveAbility);
                }
                else
                {
                    AttackAbility attackAbility = (AttackAbility)ability;
                    abilities.Add(attackAbility.Id, attackAbility);
                }
            }
        }

        public static void AddAbility(Ability newAbility)
        {
            if (collection == null)
            {
                Console.WriteLine("no collection");
                return;
            }
            Console.WriteLine("Adding Ability");
            

            AttackAbility attackAbility = newAbility as AttackAbility;
            DefensiveAbility defensiveAbility = newAbility as DefensiveAbility;

            if (attackAbility != null)
            {
                abilities.Add(attackAbility.Id, attackAbility);
                collection.InsertOne(attackAbility);
            }
            else if (defensiveAbility != null)
            {
                abilities.Add(defensiveAbility.Id, defensiveAbility);
                collection.InsertOne(defensiveAbility);
            }
            Console.WriteLine("Ability added");
        }
        internal static void SetCollection(IMongoDatabase database)
        {            
            collection = database.GetCollection<Ability>(collectionName);
        }
    }
}
