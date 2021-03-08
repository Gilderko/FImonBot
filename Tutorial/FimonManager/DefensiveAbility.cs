﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FImons
{   
    public class DefensiveAbility : Ability
    {
        public DefensiveAbility(ulong id, AbilityType abilityType, ElementalTypes abilityForm, string name, string description, int cost, int charges, int? healVal = null) : base(id,abilityType,abilityForm,name,description,cost)
        {
            HealValue = healVal;
            Charges = charges;
        }

        [BsonElement("heal_value")]
        public int? HealValue { get; set; }

        [BsonElement("charges")]
        public int Charges { get; set; }

        public string GetDescriptionForMessage()
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization\nHeals for: {HealValue}\n Energy cost: {AbilityCost} \nCharges: {Charges}";
            return data;
        }

        public string GetDescriptionWithFImon(InCombatFImon FImonBase)
        {
            string data = $"{Description}\nThe ability is of {ElementalType.ToString()} specialization\nHeals for: {GetHealValueWithFImon(FImonBase.FImonBase)}\n Energy cost: {GetCostWithFImon(FImonBase)} \nCharges: {FImonBase.defensiveCharges}";
            return data;
        }

        public int GetHealValueWithFImon(FImon FImonBase)
        {
            if (HealValue != null)
            {
                return (int)(HealValue * (1 + FImonBase.AbilityPower * BaseStats.abilityPowerIntensityIncrease / 100f));
            }
            return 0;
        }
    }
}
