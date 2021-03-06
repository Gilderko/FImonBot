using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FImons
{
    public class InCombatFImon
    {
        public const int START_HEALTH = 150;
        public const int START_ENERGY = 115;

        public int maxHealth;
        public int maxEnergy;

        public int health;
        public int energy;

        public FImon FImonBase;

        public InCombatFImon(FImon baseFImon)
        {
            FImonBase = baseFImon;
            health = (int) (START_HEALTH * (1 + FImonBase.Stamina*BaseStats.staminaHealthIncrease/100f - FImonBase.Agility*BaseStats.agilityHealthDecrease/100f));
            maxHealth = health;
            energy = (int) (START_ENERGY * (1 + FImonBase.Stamina*BaseStats.staminaEnergyIncrease / 100f));
            maxEnergy = energy;
        }

        public int UseAbilityFImon(Ability abilityToUse, InCombatFImon enemyFImon, out string commentary)
        {
            if (abilityToUse == null)
            {
                WaitTurn();
                commentary = $"{FImonBase.Name} has decided to wait a turn to replenish 20% of his energy";
                return 0;
            }
            else if (abilityToUse.AbilityType == AbilityType.DefensiveAbility)
            {
                DefensiveAbility defensiveAbility = (DefensiveAbility)abilityToUse;
                health = Math.Clamp(health + defensiveAbility.GetHealValueWithFImon(FImonBase), 0, maxHealth);
                energy -= defensiveAbility.GetCostWithFImon(FImonBase);
                commentary = $"{FImonBase.Name} has decided to heal himself for {defensiveAbility.GetHealValueWithFImon(FImonBase)}";
                return 0;
            }
            else
            {
                return AttackFImon(abilityToUse, enemyFImon, out commentary);

            }
        }

        private int AttackFImon(Ability abilityToUse, InCombatFImon enemyFImon, out string commentary)
        {
            commentary = "";

            AttackAbility attackAbility = (AttackAbility)abilityToUse;       
            Random random = new Random();

            energy -= attackAbility.GetCostWithFImon(FImonBase);

            int hitChance = attackAbility.GetHitChanceWithFImon(FImonBase);
            int dodgeChance = enemyFImon.GetDodgeChance();
            int didHit = random.Next(1, 101);
            if (didHit > hitChance - dodgeChance)
            {
                commentary = $"'{FImonBase.Name}' misses '{enemyFImon.FImonBase.Name}' with '{attackAbility.Name}' of type '{attackAbility.ElementalType}'";
                return 0;
            }

            int damageValue = random.Next(attackAbility.GetLowerDamageWithFImon(FImonBase), attackAbility.GetUpperDamageWithFImon(FImonBase) + 1);
            string elementAffection = "";

            if (BaseStats.IsStrongAgainst(attackAbility.ElementalType, enemyFImon.FImonBase.PrimaryType))
            {
                elementAffection = "Used ELEMENT is SUPER effective";
                damageValue = (int)(damageValue * (1 + BaseStats.primaryTypeModifier / 100f));
            }
            else if (BaseStats.IsStrongAgainst(attackAbility.ElementalType, enemyFImon.FImonBase.SecondaryType))
            {
                elementAffection = "Used ELEMENT is SUPER effective";
                damageValue = (int)(damageValue * (1 + BaseStats.secondaryTypeModifier / 100f));
            }

            if (BaseStats.IsStrongAgainst(enemyFImon.FImonBase.PrimaryType, attackAbility.ElementalType))
            {
                elementAffection = "Used ELEMENT NOT effective";
                damageValue = (int)(damageValue * (1 - BaseStats.primaryTypeModifier / 100f));
            }
            else if (BaseStats.IsStrongAgainst(enemyFImon.FImonBase.SecondaryType, attackAbility.ElementalType))
            {
                elementAffection = "Used ELEMENT is NOT effective";
                damageValue = (int)(damageValue * (1 - BaseStats.secondaryTypeModifier / 100f));
            }

            int critChance = FImonBase.Luck * BaseStats.luckCritChanceIncrease;
            critChance = (int)(critChance - (FImonBase.Inteligence * BaseStats.inteligenceCritChanceDecrease / 100f));
            int isCriticalhit = random.Next(1, 101);
            string critHitCommentary = "";
            if (isCriticalhit <= critChance)
            {
                critHitCommentary = "Critical HIT\n";
                damageValue = (int)(damageValue * 1.5);
            }
            commentary += critHitCommentary;
            commentary += elementAffection;
            commentary += $"\n'{FImonBase.Name}' strikes '{enemyFImon.FImonBase.Name}' for '{damageValue}' with '{attackAbility.Name}' of type '{attackAbility.ElementalType}'";
            
            
            return damageValue;
        }

        public bool HaveEnoughEnergyForAbility(Ability abilityToUse)
        {
            int cost = abilityToUse.GetCostWithFImon(FImonBase);            
            if (cost > energy)
            {
                return false;
            }
            return true;
        }

        public int GetDodgeChance()
        {
            return FImonBase.Agility * BaseStats.agilityDodgeChanceIncrease + FImonBase.Perception * BaseStats.perceptionDodgeChanceDecrease;
        }
        public void WaitTurn()
        {
            energy = Math.Clamp(energy + (int)(maxHealth * 0.2f), 0, maxEnergy);
        }   
    }
}
