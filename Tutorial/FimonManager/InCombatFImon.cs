using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FImons
{
    public class InCombatFImon
    {
        public const int START_HEALTH = 100;
        public const int START_ENERGY = 100;

        public int maxHealth;
        public int maxEnergy;

        public int health;
        public int energy;

        public FImon FImonBase;

        public InCombatFImon(FImon baseFImon)
        {
            FImonBase = baseFImon;
            health = (int) (START_HEALTH * (1 + BaseStats.staminaHealthIncrease/100f - BaseStats.agilityHealthDecrease/100f));
            maxHealth = health;
            energy = (int) (START_ENERGY * (1 + BaseStats.staminaEnergyIncrease / 100f));
            maxEnergy = energy;
        }

        public int UseAbilityFImon(Ability abilityToUse, InCombatFImon enemyFImon, out string commentary)
        {
            if (abilityToUse.Id == 0)
            {
                WaitTurn();
                commentary = $"{FImonBase.Name} has decided to wait a turn to replenish 20% of his energy";
                return 0;
            }
            else if (abilityToUse.AbilityType == AbilityType.DefensiveAbility)
            {
                DefensiveAbility defensiveAbility = (DefensiveAbility)abilityToUse;
                commentary = "Defensive ability is not yet implemented... so go frick yourself";
                return 10;
            }
            else
            {
                return AttackFImon(abilityToUse, enemyFImon, out commentary);
            }
        }

        private int AttackFImon(Ability abilityToUse, InCombatFImon enemyFImon, out string commentary)
        {
            AttackAbility attackAbility = (AttackAbility)abilityToUse;

            // check if hit
            Random random = new Random();
            int hitChance = attackAbility.HitChance + FImonBase.Perception * BaseStats.perceptionHitChanceIncrease;
            int dodgeChance = FImonBase.Agility * BaseStats.agilityDodgeChanceIncrease;
            int didHit = random.Next(1, 101);
            if (didHit > hitChance - dodgeChance)
            {
                commentary = $"{FImonBase.Name} misses {enemyFImon.FImonBase.Name} with {attackAbility.Name} of type {attackAbility.ElementalType}";
                return 0;
            }

            // add damage through attributes
            int damageValue = random.Next(attackAbility.DamageValueLower, attackAbility.DamageValueUpper + 1);
            if (attackAbility.AbilityType == AbilityType.AutoAttack)
            {
                damageValue = (int)(damageValue * (1 + FImonBase.Strength * BaseStats.strengthAutoAttackDamageIncrease / 100f));
            }
            else
            {
                damageValue = (int)(damageValue * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerDamageIncrease / 100f));
            }

            // add damage through enemy elemental type
            if (BaseStats.IsStrongAgainst(attackAbility.ElementalType, enemyFImon.FImonBase.PrimaryType))
            {
                damageValue = (int)(damageValue * (1 + BaseStats.primaryTypeModifier / 100f));
            }
            else if (BaseStats.IsStrongAgainst(attackAbility.ElementalType, enemyFImon.FImonBase.SecondaryType))
            {
                damageValue = (int)(damageValue * (1 + BaseStats.secondaryTypeModifier / 100f));
            }

            if (BaseStats.IsStrongAgainst(enemyFImon.FImonBase.PrimaryType, attackAbility.ElementalType))
            {
                damageValue = (int)(damageValue * (1 - BaseStats.primaryTypeModifier / 100f));
            }
            else if (BaseStats.IsStrongAgainst(enemyFImon.FImonBase.SecondaryType, attackAbility.ElementalType))
            {
                damageValue = (int)(damageValue * (1 - BaseStats.secondaryTypeModifier / 100f));
            }

            // check if critical hit
            int critChance = FImonBase.Luck * BaseStats.luckCritChanceIncrease;
            critChance = (int)(critChance - (FImonBase.Inteligence * BaseStats.inteligenceCritChanceDecrease / 100f));
            int isCriticalhit = random.Next(1, 101);
            if (isCriticalhit <= critChance)
            {
                damageValue = (int)(damageValue * 1.5);
            }

            commentary = $"{FImonBase.Name} strikes {enemyFImon.FImonBase.Name} for {damageValue} with {attackAbility.Name} of type {attackAbility.ElementalType}";
            return damageValue;
        }

        public bool HaveEnoughEnergyForAbility(Ability abilityToUse)
        {
            int cost = abilityToUse.AbilityCost;
            if (abilityToUse.AbilityType == AbilityType.AutoAttack)
            {
                cost = (int)(cost * (1 + FImonBase.Strength * BaseStats.strengthAutoAttackCostIncrease / 100f));

            }
            else
            {
                cost = (int)(cost * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerCostIncrease / 100f));                
            }
            
            if (cost > energy)
            {
                return false;
            }
            return true;
        }

        public void WaitTurn()
        {
            energy = Math.Clamp(energy + (int)(energy * 0.2f), 0, maxEnergy);
        }   
    }
}
