using System;
using System.Collections.Generic;
using System.Linq;

namespace FImonBotDiscord.Game.Stats
{
    public static class BaseStats
    {
        public static List<int> levelExperienceRequirements = new List<int>();
        public static readonly string[] attibutesOptions = {"strength", "stamina", "ability power", "inteligence", "luck", "agility", "perception"};

        public const int strengthAutoAttackDamageIncrease = 4;
        public const int strengthAutoAttackCostIncrease = 3;

        public const int abilityPowerIntensityIncrease = 3;
        public const int abilityPowerCostIncrease = 2;

        public const int staminaHealthIncrease = 3;
        public const int staminaEnergyIncrease = 2;

        public const int agilityDodgeChanceIncrease = 2;
        public const int agilityHealthDecrease = 2;

        public const int perceptionHitChanceIncrease = 2;
        public const int perceptionDodgeChanceDecrease = 1;

        public const int luckCritChanceIncrease = 2;
        public const int luckExpGainDecrease = 2;

        public const int inteligenceExpGainIncrease = 4;
        public const int inteligenceCritChanceDecrease = 1;

        public const int primaryTypeModifier = 15;
        public const int secondaryTypeModifier = 10;

        public const int energyGainWait = 25;
        public const int abilityPointsToAddOnLevelUp = 2;

        public static int TellLevel(int experience)
        {
            int index = 0;
            foreach (var levelReq in levelExperienceRequirements)
            {
                if (experience > levelReq)
                {
                    index++;
                    continue;
                }
                else if (experience == levelReq)
                {
                    break;
                }
                else
                {
                    index--;
                    break;
                }
            }
            return index + 1;
        }

        public static void InitialiseBaseStats(int[] levelReq)
        {
            foreach (var req in levelReq)
            {
                Console.WriteLine(req);
            }
            levelExperienceRequirements = levelReq.ToList();
        }

        public static bool IsStrongAgainst(ElementalTypes attackingType, ElementalTypes defendingType)
        {
            switch (attackingType)
            {
                case ElementalTypes.Fire:
                    return defendingType == ElementalTypes.Air;
                case ElementalTypes.Air:
                    return defendingType == ElementalTypes.Water;
                case ElementalTypes.Water:
                    return defendingType == ElementalTypes.Fire || defendingType == ElementalTypes.Ground;
                case ElementalTypes.Ground:
                    return defendingType == ElementalTypes.Air;
                case ElementalTypes.Steel:
                    return defendingType == ElementalTypes.Fire;
                default:
                    return false;
            }
        }
    }
}
