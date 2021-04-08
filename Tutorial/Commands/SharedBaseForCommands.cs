using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using FImonBotDiscord.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FImonBotDiscord.Game.Abilities;
using FImonBotDiscord.Game.Combat;
using FImonBotDiscord.Game.FImons;
using FImonBotDiscord.Game.Stats;
using FImonBotDiscord.Game.Trainers;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using FImonBotDiscord.Game;
using FImonBotDiscord.Handlers.Dialogue;

namespace FImonBotDiscord.Commands
{
    public class SharedBaseForCommands : BaseCommandModule
    {
        public const ulong authorID = 317634903959142401;

        [Command("ping")] // The actual string they have to type to trigger it
        [Description("Returns pong")]
        private async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        protected Dictionary<DiscordEmoji, ReactionStepData> GenerateAttackOptions(CommandContext ctx, InCombatFImon currentFImon)
        {
            AttackAbility autoAttack = currentFImon.FImonBase.AutoAttack;
            AttackAbility basicAttack = currentFImon.FImonBase.BasicAttack;
            AttackAbility specialAttack = currentFImon.FImonBase.SpecialAttack;
            AttackAbility finalAttack = currentFImon.FImonBase.FinalAttack;
            DefensiveAbility defensiveAbility = currentFImon.FImonBase.DefensiveAbility;

            FImon FImonBase = currentFImon.FImonBase;
            var options = new Dictionary<DiscordEmoji, ReactionStepData>();

            if (FImonBase.AutoAttack != null && currentFImon.HaveEnoughEnergyForAbility(autoAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":crossed_swords:"), new ReactionStepData { Content = $"{autoAttack.Name}\n" + autoAttack.GetDescriptionWithFImon(FImonBase), NextStep = null, optionalData = autoAttack.Id });
            }
            if (FImonBase.BasicAttack != null && currentFImon.HaveEnoughEnergyForAbility(basicAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":one:"), new ReactionStepData { Content = $"{basicAttack.Name}\n" + basicAttack.GetDescriptionWithFImon(FImonBase), NextStep = null, optionalData = basicAttack.Id });
            }
            if (FImonBase.SpecialAttack != null && currentFImon.HaveEnoughEnergyForAbility(specialAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":two:"), new ReactionStepData { Content = $"{specialAttack.Name}\n" + specialAttack.GetDescriptionWithFImon(FImonBase), NextStep = null, optionalData = specialAttack.Id });
            }
            if (FImonBase.FinalAttack != null && currentFImon.HaveEnoughEnergyForAbility(finalAttack))
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":three:"), new ReactionStepData { Content = $"{finalAttack.Name}\n" + finalAttack.GetDescriptionWithFImon(FImonBase), NextStep = null, optionalData = finalAttack.Id });
            }
            if (FImonBase.DefensiveAbility != null && currentFImon.HaveEnoughEnergyForAbility(defensiveAbility) && currentFImon.DefensiveCharges > 0)
            {
                options.Add(DiscordEmoji.FromName(ctx.Client, ":shield:"), new ReactionStepData { Content = $"{defensiveAbility.Name}\n" + defensiveAbility.GetDescriptionWithFImon(FImonBase), NextStep = null, optionalData = defensiveAbility.Id });
            }

            ulong def = 0;
            options.Add(DiscordEmoji.FromName(ctx.Client, ":zzz:"), new ReactionStepData { Content = $"Skip turn to generate {BaseStats.energyGainWait}% energy", NextStep = null, optionalData = def });
            return options;
        }

        protected DiscordEmbedBuilder GenerateFImonEmbed(Trainer trainer, InCombatFImon currentFightingFImon, bool displayAttacks)
        {
            var optionalEmbed = new DiscordEmbedBuilder();
            optionalEmbed.AddField($"FImon of owner {trainer.Name}", currentFightingFImon.FImonBase.Name);
            optionalEmbed.AddField("Description", currentFightingFImon.FImonBase.Description);
            optionalEmbed.AddField($"Dodge chance", currentFightingFImon.GetDodgeChance().ToString());
            optionalEmbed.AddField("Health", $"{currentFightingFImon.CurrentHealth.ToString()}/{currentFightingFImon.MaxHealth}");
            optionalEmbed.AddField("Energy", $"{currentFightingFImon.CurrentEnergy.ToString()}/{currentFightingFImon.MaxEnergy}");
            optionalEmbed.AddField("Primary type, Secondary type", $"{currentFightingFImon.FImonBase.PrimaryType}, {currentFightingFImon.FImonBase.SecondaryType}");

            if (displayAttacks)
            {
                var FImonBase = currentFightingFImon.FImonBase;
                AttackAbility autoAttack = currentFightingFImon.FImonBase.AutoAttack;
                AttackAbility basicAttack = currentFightingFImon.FImonBase.BasicAttack;
                AttackAbility specialAttack = currentFightingFImon.FImonBase.SpecialAttack;
                AttackAbility finalAttack = currentFightingFImon.FImonBase.FinalAttack;
                DefensiveAbility defensiveAbility = currentFightingFImon.FImonBase.DefensiveAbility;
                if (FImonBase.AutoAttack != null)
                {
                    optionalEmbed.AddField($"{autoAttack.Name}\n", autoAttack.GetDescriptionWithFImon(FImonBase));
                }
                if (FImonBase.BasicAttack != null)
                {
                    optionalEmbed.AddField($"{basicAttack.Name}\n", basicAttack.GetDescriptionWithFImon(FImonBase));
                }
                if (FImonBase.SpecialAttack != null)
                {
                    optionalEmbed.AddField($"{specialAttack.Name}\n", specialAttack.GetDescriptionWithFImon(FImonBase));
                }
                if (FImonBase.FinalAttack != null)
                {
                    optionalEmbed.AddField($"{finalAttack.Name}\n", finalAttack.GetDescriptionWithFImon(FImonBase));
                }
                if (FImonBase.DefensiveAbility != null)
                {
                    optionalEmbed.AddField($"{defensiveAbility.Name}\n", defensiveAbility.GetDescriptionWithFImon(FImonBase));
                }
            }
            return optionalEmbed;
        }

        protected Dictionary<DiscordEmoji, ReactionStepData> GenerateFImonOptions(DiscordClient discordClient, Trainer trainer)
        {
            var option = new Dictionary<DiscordEmoji, ReactionStepData>();

            if (trainer.FImon1 != null)
            {
                option.Add(DiscordEmoji.FromName(discordClient, ":one:"), new ReactionStepData { Content = $"To preview {trainer.FImon1.Name}: \nDescription: {trainer.FImon1.Description}", NextStep = null, optionalData = trainer.FImon1 });
            }
            if (trainer.FImon2 != null)
            {
                option.Add(DiscordEmoji.FromName(discordClient, ":two:"), new ReactionStepData { Content = $"To preview {trainer.FImon2.Name}: \nDescription: {trainer.FImon2.Description}", NextStep = null, optionalData = trainer.FImon2 });
            }
            if (trainer.FImon3 != null)
            {
                option.Add(DiscordEmoji.FromName(discordClient, ":three:"), new ReactionStepData { Content = $"To preview {trainer.FImon3.Name}: \nDescription: {trainer.FImon3.Description}", NextStep = null, optionalData = trainer.FImon3 });
            }
            if (trainer.FImon4 != null)
            {
                option.Add(DiscordEmoji.FromName(discordClient, ":four:"), new ReactionStepData { Content = $"To preview {trainer.FImon4.Name}: \nDescription: {trainer.FImon4.Description}", NextStep = null, optionalData = trainer.FImon4 });
            }

            return option;
        }

        protected static Dictionary<DiscordEmoji, ReactionStepData> GetFImonTypesReactionOptions(CommandContext ctx)
        {
            return new Dictionary<DiscordEmoji, ReactionStepData>
            {
                { DiscordEmoji.FromName(ctx.Client,":fire:"), new ReactionStepData{ Content = "Fire Type", NextStep = null, optionalData = ElementalTypes.Fire }},
                { DiscordEmoji.FromName(ctx.Client,":potable_water:"), new ReactionStepData{ Content = "Water Type", NextStep = null, optionalData = ElementalTypes.Water}},
                { DiscordEmoji.FromName(ctx.Client,":earth_africa:"), new ReactionStepData{ Content = "Ground Type", NextStep = null, optionalData = ElementalTypes.Ground}},
                { DiscordEmoji.FromName(ctx.Client,":cloud_tornado:"), new ReactionStepData{ Content = "Air Type", NextStep = null, optionalData = ElementalTypes.Air}},
                { DiscordEmoji.FromName(ctx.Client,":steam_locomotive:"), new ReactionStepData{ Content = "Steel Type", NextStep = null, optionalData = ElementalTypes.Steel}}
            };
        }

        protected static DiscordEmbedBuilder AttributesEmbedInfo()
        {
            var attributesIntroEmbed = new DiscordEmbedBuilder()
            {
                Title = "Attributes Selection Intro",
                Color = DiscordColor.DarkButNotBlack,
                Description = "You get to choose from 7 different attributes where each attribute improves certain properties of your FImon, but decreases others. Maximum amount " +
                            "of point you can insert into an attribute is 10 a minimum is 1.\nPOINTS WHICH YOU WILL NOT USE WILL BE DISCARDED!!!"
            };

            attributesIntroEmbed.AddField("Strength", $"each point increases your auto-attack damage by {BaseStats.strengthAutoAttackDamageIncrease}% and increases its cost by {BaseStats.strengthAutoAttackCostIncrease}%");
            attributesIntroEmbed.AddField("Stamina", $"each point increases your energy pool by {BaseStats.staminaEnergyIncrease}% and your health pool by {BaseStats.staminaHealthIncrease}%");
            attributesIntroEmbed.AddField("Inteligence", $"each point increases experience gained by {BaseStats.inteligenceExpGainIncrease}%, but decreases your chance to critically hit by {BaseStats.inteligenceCritChanceDecrease}%");
            attributesIntroEmbed.AddField("Luck", $"each point increases your chance to critically hit by {BaseStats.luckCritChanceIncrease}%, but decreases your experience gained by {BaseStats.luckExpGainDecrease}%");
            attributesIntroEmbed.AddField("Agility", $"each point increases your chane to dodge by {BaseStats.agilityDodgeChanceIncrease}%, but decreases your health by {BaseStats.agilityHealthDecrease}%");
            attributesIntroEmbed.AddField("Perception", $"each point increases your chance to hit by {BaseStats.perceptionHitChanceIncrease}%, but decreases your chance to dodge by {BaseStats.perceptionDodgeChanceDecrease}%");
            attributesIntroEmbed.AddField("Ability power", $"each point increases the damage, healing of all your non AUTO-ATTACK abilities by {BaseStats.abilityPowerIntensityIncrease}% and increases their cost by {BaseStats.abilityPowerCostIncrease}%");
            return attributesIntroEmbed;
        }

        protected static void GenerateAttacksChoiceOptions(Dictionary<string, TextChoiceData> options, AbilityType abilityType)
        {
            foreach (var ability in AbilityManager.GetAttackAbilities())
            {
                if (ability.AbilityType == abilityType)
                {
                    options.Add(ability.Name, new TextChoiceData(ability.GetDescriptionForMessage(), ability.Id));
                }
            }
        }

        protected static void GenerateDefenseChoiceOption(Dictionary<string, TextChoiceData> options, AbilityType abilityType)
        {
            foreach (var ability in AbilityManager.GetDefensiveAbilities())
            {
                if (ability.AbilityType == abilityType)
                {
                    options.Add(ability.Name, new TextChoiceData(ability.GetDescriptionForMessage(), ability.Id));
                }
            }
        }

        protected async Task<FImon> SelectYourFImon(DiscordUser discordUser, DiscordChannel discordChannel, DiscordClient discordClient, DiscordUser responder = null)
        {
            Trainer trainer = TrainerManager.GetTrainer(discordUser.Id);

            if (trainer == null)
            {
                await discordChannel.SendMessageAsync("Sadly you don´t have a trainer");
                return null;
            }
            if (!trainer.HasFImon())
            {
                await discordChannel.SendMessageAsync("Sadly you don´t have any FImons");
                return null;
            }

            var options = GenerateFImonOptions(discordClient, trainer);

            var chooseStep = new ReactionStep("Which FImon do you want to select?", options);
            FImon selectedFImon = null;
            chooseStep.OnValidResult += (result) =>
            {
                selectedFImon = (FImon)options[result].optionalData;
            };

            var inputDialogueHandler = new DialogueHandler(discordClient, discordChannel, responder, chooseStep, false, false);
            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                return null;
            }
            return selectedFImon;
        }

        protected async Task SendErrorMessage(string errorMessage, CommandContext ctx)
        {
            var errMessage = await ctx.Channel.SendMessageAsync($"`{errorMessage}`");
            await Task.Delay(5000);
            await errMessage.DeleteAsync();
        }

        protected async Task<ulong> SendCorrectMessage(string correctMessage, CommandContext ctx)
        {
            return (await ctx.Channel.SendMessageAsync($"`{correctMessage}`")).Id;
        }

        protected async Task<ulong> SendCorrectMessage(DiscordEmbed embedMessage, CommandContext ctx)
        {
            return (await ctx.Channel.SendMessageAsync(embed: embedMessage)).Id;
        }

        protected async Task<bool> ConfirmPreviousStep(CommandContext ctx, DiscordUser responderOverride)
        {
            var options = new Dictionary<DiscordEmoji, ReactionStepData>()
            {
                { DiscordEmoji.FromName(ctx.Client,":white_check_mark:"), new ReactionStepData{ Content = "Confirm", NextStep = null}},                
            };
            var reactionStep = new ReactionStep("Do you want to proceed?", options);

            var inputDialogueHandler = new DialogueHandler(ctx.Client, ctx.Channel, responderOverride, reactionStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            return succeeded;
        }
    }
}
