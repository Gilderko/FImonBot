using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FImonBot.CommandAttributes;
using FImonBot.Game;
using FImonBot.Game.Abilities;
using FImonBot.Game.FImons;
using FImonBot.Handlers.Dialogue;
using FImonBot.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FImonBot.Commands
{
    public class AbilityCommands : SharedBaseForCommands
    {
        [Command("setAllAbilities")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task SetAllAbilites(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.AutoAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.BasicAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.SpecialAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.FinalAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.DefensiveAbility);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        public async Task SetAbility(CommandContext ctx, FImon fImon, AbilityType abilityType)
        {
            var options = new Dictionary<string, TextChoiceData>();
            if (abilityType == AbilityType.DefensiveAbility)
            {
                GenerateDefenseChoiceOption(options, abilityType);
            }
            else
            {
                GenerateAttacksChoiceOptions(options, abilityType);
            }

            var attackSetStep = new TextChoiceStep($"Select your {abilityType.ToString()}", null, options);
            ulong abilityID = 0;
            attackSetStep.OnValidResult = (result) =>
            {
                abilityID = (ulong)result.OptionalData;
            };

            attackSetStep.SetNextStep(null);

            var userChannel = await ctx.Member.CreateDmChannelAsync();
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, attackSetStep, false, true);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);
            Console.WriteLine(abilityID);

            if (!succeeded)
            {
                return;
            }

            fImon.SetNewAbility(AbilityManager.GetAbility(abilityID));
        }

        [Command("setautoattack")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task SetAutoAttack(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.AutoAttack);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [Command("setbasicattack")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task SetBasicAttack(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.BasicAttack);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [Command("setspecialattack")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task SetSpecialAttack(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.SpecialAttack);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [Command("setfinalattack")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task SetFinalAttack(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.FinalAttack);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [Command("setdefensive")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task SetDefensiveAbility(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.DefensiveAbility);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [Command("initialiseAbilities")]
        [RequireChannelNameIncludes("afk")]
        [RequireAdmin]
        public async Task InitialiseAbilities(CommandContext ctx)
        {
            AttackAbility att1 = new AttackAbility(1, AbilityType.AutoAttack, ElementalTypes.Ground,
                "Punch", "Average punch of FI student", 20, 20, 10, 65, 15);
            AttackAbility att2 = new AttackAbility(2, AbilityType.AutoAttack, ElementalTypes.Fire,
                "Kick", "Jan Claud van Damn Kick", 15, 20, 5, 60, 25);
            AttackAbility att3 = new AttackAbility(3, AbilityType.AutoAttack, ElementalTypes.Air,
                "Scratch", "Scratches you like your girlfriend... so not at all", 10, 15, 5, 75, 0);
            //-----------------------
            AttackAbility att4 = new AttackAbility(4, AbilityType.BasicAttack, ElementalTypes.Water,
                "Water gun", "Almighty stream of sodastream water", 15, 20, 16, 70, 10);
            AttackAbility att5 = new AttackAbility(5, AbilityType.BasicAttack, ElementalTypes.Fire,
                "FI Roast", "Average roast you get from a FI student", 20, 15, 5, 75, 70);
            AttackAbility att6 = new AttackAbility(6, AbilityType.BasicAttack, ElementalTypes.Ground,
                "Matematika Drsně a svižně", "Swift attack with a previously mentioned book", 18, 18, 10, 80, 45);
            //-----------------------
            AttackAbility att7 = new AttackAbility(7, AbilityType.SpecialAttack, ElementalTypes.Fire,
                "Odpovednik", "Yet another odpovedník", 35, 35, 20, 50, 25);
            AttackAbility att8 = new AttackAbility(8, AbilityType.SpecialAttack, ElementalTypes.Air,
                "Sleeping powder", "The powder of thats made from tears of PB152 students", 40, 30, 15, 55, 50);
            AttackAbility att9 = new AttackAbility(9, AbilityType.SpecialAttack, ElementalTypes.Steel,
                "Naprosto ez xd", "The greatest line to ever exist", 30, 40, 10, 50, 35);
            //-----------------------
            AttackAbility att10 = new AttackAbility(10, AbilityType.FinalAttack, ElementalTypes.Fire,
                "Really ni**a?", "The almighty question of FI", 75, 55, 40, 65, 10);
            AttackAbility att11 = new AttackAbility(11, AbilityType.FinalAttack, ElementalTypes.Air,
                "Kontr strike", "Sadly test neprošel", 80, 35, 30, 60, 90);
            AttackAbility att12 = new AttackAbility(12, AbilityType.FinalAttack, ElementalTypes.Steel,
                "Bretuna?", "Tunabre...", 65, 70, 35, 55, 0);
            //-----------------------
            DefensiveAbility def1 = new DefensiveAbility(13, AbilityType.DefensiveAbility, ElementalTypes.Steel,
                "Harden", "Gets your... thing... even harder", 55, 1, 75);
            DefensiveAbility def3 = new DefensiveAbility(14, AbilityType.DefensiveAbility, ElementalTypes.Air,
                "Basic Heal", "Heal for moderate amount", 25, 2, 25);

            AbilityManager.AddAbility(att1);
            AbilityManager.AddAbility(att2);
            AbilityManager.AddAbility(att3);
            AbilityManager.AddAbility(att4);
            AbilityManager.AddAbility(att5);
            AbilityManager.AddAbility(att6);
            AbilityManager.AddAbility(att7);
            AbilityManager.AddAbility(att8);
            AbilityManager.AddAbility(att9);
            AbilityManager.AddAbility(att10);
            AbilityManager.AddAbility(att11);
            AbilityManager.AddAbility(att12);
            AbilityManager.AddAbility(def1);
            AbilityManager.AddAbility(def3);

            await SendCorrectMessage("Added abilities", ctx.Channel);
        }

        [Command("getabilities")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireAdmin]
        public async Task GetAbilities(CommandContext ctx)
        {
            foreach (var ab in AbilityManager.GetAttackAbilities())
            {
                Console.WriteLine(ab.Name);
                Console.WriteLine(ab.Id);
            }
            foreach (var ab in AbilityManager.GetDefensiveAbilities())
            {
                Console.WriteLine(ab.Name);
                Console.WriteLine(ab.Id);
            }
        }
    }
}
