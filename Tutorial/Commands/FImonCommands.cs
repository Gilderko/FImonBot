using Discord_Bot_Tutorial.Attributes;
using Discord_Bot_Tutorial.Handlers.Dialogue;
using Discord_Bot_Tutorial.Handlers.Dialogue.Steps;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tutorial.Game;
using Tutorial.Game.FImons;
using Tutorial.Game.Abilities;
using Tutorial.Game.Combat;
using Tutorial.Game.Stats;
using Tutorial.Game.Trainers;
using DSharpPlus;

namespace Discord_Bot_Tutorial.Commands
{
    public class FImonCommands : BaseCommandModule
    {
        [Command("ping")] // The actual string they have to type to trigger it
        [Description("Returns pong")]
        [RequireCategoriesAttribute(ChannelCheckMode.Any, "Text Channels")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false); // Await means it wont continue until it is done
        }
        
        [Command("addfimon")]
        public async Task AddFimonCommand(CommandContext ctx)
        {
            var userID = ctx.User.Id;
            var trainer = TrainerManager.GetTrainer(userID);
            if (trainer == null)
            {
                await ctx.Channel.SendMessageAsync("Mate... you dont have a trainer yet").ConfigureAwait(false);
                return;
            }
            if (!trainer.CanAddFImon())
            {
                await ctx.Channel.SendMessageAsync("Mate... you already have max amount of FImons").ConfigureAwait(false);
                return;
            }

            var nameStep = new TextStep("Welcome \nPlease choose the name for your FImon", null, 1, 30);
            string FImonName = "";
            nameStep.OnValidResult += (result) => FImonName = result;

            var descriptionStep = new TextStep("Great... Now write a short description for your FImon", null, 10, 200);
            string description = "";
            descriptionStep.OnValidResult += (result) => description = result;
            Dictionary<DiscordEmoji, ReactionStepData> reactionTypeOptions = GetFImonTypesReactionOptions(ctx);

            var primaryTypeStep = new ReactionStep("Now what is your primary type?", reactionTypeOptions);
            ElementalTypes primaryType = ElementalTypes.Air;
            primaryTypeStep.OnValidResult += (result) =>
            {
                primaryType = (ElementalTypes)reactionTypeOptions[result].optionalData;
                reactionTypeOptions.Remove(result);
            };

            var secondaryTypeStep = new ReactionStep("Now what is your secondary type?", reactionTypeOptions);
            ElementalTypes secondaryType = ElementalTypes.Air;
            secondaryTypeStep.OnValidResult += (result) =>
            {
                secondaryType = (ElementalTypes)reactionTypeOptions[result].optionalData;
                reactionTypeOptions.Remove(result);
            };

            DiscordEmbedBuilder attributesIntroEmbed = AttributesEmbedInfo();

            var introToAttributes = new ReactionStep("", new Dictionary<DiscordEmoji, ReactionStepData>
            {
                { DiscordEmoji.FromName(ctx.Client,":thumbsup:"), new ReactionStepData{ Content = "Continue" } }
            });
            introToAttributes.optionalEmbed = attributesIntroEmbed;

            int pointsToUse = 35;
            int lowBound = 1;
            int highBound = 10;

            var strengthStep = new IntStep("Please state a value of you strength", null, lowBound, highBound);
            strengthStep.dynamicOptionalCommentary = pointsToUse.ToString();
            int strengthValue = 1;
            strengthStep.OnValidResult += (result) =>
            {
                strengthValue = result;
                pointsToUse -= result;
                ((IntStep)strengthStep.GetNextStep())._maxValue = highBound;
                ((IntStep)strengthStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var staminaStep = new IntStep("Please state a value of you stamina", null, lowBound, highBound);
            int staminaValue = 1;
            staminaStep.OnValidResult += (result) =>
            {
                staminaValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 4 > 10 ? highBound : pointsToUse - 4;
                ((IntStep)staminaStep.GetNextStep())._maxValue = highBound;
                ((IntStep)staminaStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var inteligenceStep = new IntStep("Please state a value of you inteligence", null, lowBound, highBound);
            int inteligenceValue = 1;
            inteligenceStep.OnValidResult += (result) =>
            {
                inteligenceValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 3 > 10 ? highBound : pointsToUse - 3;
                ((IntStep)inteligenceStep.GetNextStep())._maxValue = highBound;
                ((IntStep)inteligenceStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var luckStep = new IntStep("Please state a value of you luck", null, lowBound, highBound);
            int luckValue = 1;
            luckStep.OnValidResult += (result) =>
            {
                luckValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 2 > 10 ? highBound : pointsToUse - 2;
                ((IntStep)luckStep.GetNextStep())._maxValue = highBound;
                ((IntStep)luckStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                Console.WriteLine(pointsToUse);
            };

            var agilityStep = new IntStep("Please state a value of you agility", null, lowBound, highBound);
            int agilityValue = 1;
            agilityStep.OnValidResult += (result) =>
            {
                agilityValue = result;
                pointsToUse -= result;
                highBound = pointsToUse - 1 > 10 ? highBound : pointsToUse - 1;
                ((IntStep)agilityStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                ((IntStep)agilityStep.GetNextStep())._maxValue = highBound;
            };

            var perceptionStep = new IntStep("Please state a value of you perception", null, lowBound, highBound);
            int perceptionValue = 1;
            perceptionStep.OnValidResult += (result) =>
            {
                perceptionValue = result;
                pointsToUse -= result;
                highBound = pointsToUse > 10 ? highBound : pointsToUse;
                ((IntStep)perceptionStep.GetNextStep()).dynamicOptionalCommentary = pointsToUse.ToString();
                ((IntStep)perceptionStep.GetNextStep())._maxValue = highBound;
            };

            var abilityPowerStep = new IntStep("Please state a value of you ability power", null, lowBound, highBound);
            int abilityPowerValue = 1;
            perceptionStep.OnValidResult += (result) =>
            {
                abilityPowerValue = result;
            };

            nameStep.SetNextStep(descriptionStep);
            descriptionStep.SetNextStep(primaryTypeStep);
            primaryTypeStep.SetNextStep(secondaryTypeStep);
            secondaryTypeStep.SetNextStep(introToAttributes);
            introToAttributes.SetNextStep(strengthStep);
            strengthStep.SetNextStep(staminaStep);
            staminaStep.SetNextStep(inteligenceStep);
            inteligenceStep.SetNextStep(luckStep);
            luckStep.SetNextStep(agilityStep);
            agilityStep.SetNextStep(perceptionStep);
            perceptionStep.SetNextStep(abilityPowerStep);
            abilityPowerStep.SetNextStep(null);

            var userChannel = ctx.Channel;
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, nameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) { return; }            

            FImonManager.AddFimon(trainer.TrainerID,FImonName, description, primaryType, secondaryType, strengthValue, staminaValue, inteligenceValue, luckValue,
                agilityValue, perceptionValue, abilityPowerValue);

            await ctx.Channel.SendMessageAsync("FIMON added successfully");
            await ctx.Channel.SendMessageAsync($"{FImonName} {description} {primaryType.ToString()} {secondaryType.ToString()}");
        }

        private static Dictionary<DiscordEmoji, ReactionStepData> GetFImonTypesReactionOptions(CommandContext ctx)
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

        private static DiscordEmbedBuilder AttributesEmbedInfo()
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

        [Command("initialiseAbilities")]
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

            await ctx.Channel.SendMessageAsync("Added new ability");
        }

        [Command("getabilities")]
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

        [Command("testTrainer")]
        public async Task CreateTestTrainer(CommandContext ctx)
        {
            TrainerManager.AddTrainer(ctx.User.Id, "Ash Ketchum", "The greatest FImon trainer of all time", @"https://media.comicbook.com/2016/08/ash-ketchum-194535-1280x0.jpg");
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
                abilityID = (ulong)result;
            };

            attackSetStep.SetNextStep(null);

            var userChannel = ctx.Channel;
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, attackSetStep, false, false);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);
            Console.WriteLine(abilityID);

            if (!succeeded) { return; }

            fImon.SetNewAbility(AbilityManager.GetAbility(abilityID));
            await ctx.Channel.SendMessageAsync("Ability was set to your FImon successfully");
        }

        private static void GenerateAttacksChoiceOptions(Dictionary<string, TextChoiceData> options, AbilityType abilityType)
        {
            foreach (var ability in AbilityManager.GetAttackAbilities())
            {
                if (ability.AbilityType == abilityType)
                {
                    options.Add(ability.Name, new TextChoiceData(ability.GetDescriptionForMessage(), ability.Id));
                }
            }
        }

        private static void GenerateDefenseChoiceOption(Dictionary<string, TextChoiceData> options, AbilityType abilityType)
        {
            foreach (var ability in AbilityManager.GetDefensiveAbilities())
            {
                if (ability.AbilityType == abilityType)
                {
                    options.Add(ability.Name, new TextChoiceData(ability.GetDescriptionForMessage(), ability.Id));
                }
            }
        }

        public async Task<FImon> SelectYourFImon(DiscordUser discordUser, DiscordChannel discordChannel, DiscordClient discordClient)
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

            var options = GenerateFImonOptions(discordClient,trainer);

            var chooseStep = new ReactionStep("Which FImon do you want to preview", options);
            FImon selectedFImon = null;
            chooseStep.OnValidResult += (result) =>
            {
                selectedFImon = (FImon)options[result].optionalData;
            };

            var inputDialogueHandler = new DialogueHandler(discordClient, discordChannel, discordUser, chooseStep, false,false);
            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                return null;
            }
            return selectedFImon;
        }

        [Command("setallabilities")]
        public async Task SetAllAbilites(CommandContext ctx)
        {
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);
            
            if (selectedFImon == null)
            {
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.AutoAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.BasicAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.SpecialAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.FinalAttack);
            await SetAbility(ctx, selectedFImon, AbilityType.DefensiveAbility);
        }

        [Command("setautoattack")]
        public async Task SetAutoAttack(CommandContext ctx)
        {
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.AutoAttack);
        }

        [Command("setbasicattack")]
        public async Task SetBasicAttack(CommandContext ctx)
        {
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.BasicAttack);
        }

        [Command("setspecialattack")]
        public async Task SetSpecialAttack(CommandContext ctx)
        {
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.SpecialAttack);
        }

        [Command("setfinalattack")]
        public async Task SetFinalAttack(CommandContext ctx)
        {
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.FinalAttack);
        }

        [Command("setdefensive")]
        public async Task SetDefensiveAbility(CommandContext ctx)
        {
            FImon selectedFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);

            if (selectedFImon == null)
            {
                return;
            }

            await SetAbility(ctx, selectedFImon, AbilityType.DefensiveAbility);
        }

        [Command("fight")]
        public async Task Fight(CommandContext ctx)
        {
            var mentioned = ctx.Message.MentionedUsers;            
            if (mentioned.Count != 1)
            {
                await ctx.Channel.SendMessageAsync("You have to mention one person to fight them");
                return;
            }
            var enemyUser = mentioned[0];
            var challenger = ctx.User;

            var myFImon = await SelectYourFImon(ctx.User, ctx.Channel, ctx.Client);
            if (myFImon == null)
            {
                await ctx.Channel.SendMessageAsync($"Fight can not proceed due to {challenger.Username} actions");
                return;
            }

            var enemyFImon = await SelectYourFImon(enemyUser, ctx.Channel, ctx.Client);            
            if (enemyFImon == null)
            {
                await ctx.Channel.SendMessageAsync($"Fight can not proceed due to {enemyUser.Username} actions");
                return;
            }

            if (myFImon.AutoAttackID == null || myFImon.BasicAttackID == null || myFImon.SpecialAttackID == null ||
                myFImon.FinalAttackID == null || myFImon.DefensiveAbilityID == null)
            {
                await ctx.Channel.SendMessageAsync("Please set up all your abilities before you want to fight");
                return;
            }
            if (enemyFImon.AutoAttackID == null || enemyFImon.BasicAttackID == null || enemyFImon.SpecialAttackID == null ||
                enemyFImon.FinalAttackID == null || enemyFImon.DefensiveAbilityID == null)
            {
                await ctx.Channel.SendMessageAsync("Enemy FImon does not have all of his abilities set up");
                return;
            }

            var random = new Random();
            int indexChoice = random.Next(0, 2);
            var options = new List<DiscordUser> { { challenger }, { enemyUser } };
            var optionFImon = new List<FImon> { { myFImon }, { enemyFImon } };
            
            var first = options[indexChoice];
            var second = options.Find(s => s != first);

            var firstFImon = optionFImon[indexChoice];
            var secondFImon = optionFImon.Find(s => s != firstFImon);

            var firstAttacker = new InCombatFImon(firstFImon);
            var secondAttacker = new InCombatFImon(secondFImon);

            // display embed who is going first
            var embedShow = new DiscordEmbedBuilder()
            {
                Title = $"The first attacker will be... {firstAttacker.FImonBase.Name}",
                Color = DiscordColor.DarkGreen,
            };

            InCombatFImon currentFightingFImon = firstAttacker;
            InCombatFImon helpFimon = null;
            InCombatFImon waitingFightingFImon = secondAttacker;

            DiscordUser currentFightingUser = first;
            DiscordUser helpUser = null;
            DiscordUser waitingFightingUser = second;

            InCombatFImon winningFImon = null;
            InCombatFImon loosingFImon = null;
            string battleResult = "";
            while (true)
            {
                Dictionary<DiscordEmoji, ReactionStepData> attackOptions = GenerateAttackOptions(ctx, currentFightingFImon);

                ulong AttackerAbilityID = 0;
                var AttackStep = new ReactionStep("Select your attack", attackOptions);
                AttackStep.OnValidResult += (result) =>
                {
                    AttackerAbilityID = (ulong)attackOptions[result].optionalData;
                };

                AttackStep.optionalEmbed = GenerateFImonEmbed(TrainerManager.GetTrainer(currentFightingUser.Id), currentFightingFImon, false); // FIX THIS

                var userChannel = ctx.Channel;
                var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, currentFightingUser, AttackStep, false, false);

                bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

                Console.WriteLine("finished dialogue");
                if (!succeeded)
                {
                    Console.WriteLine("Surrender");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} surrendered the fight";
                    winningFImon = waitingFightingFImon;
                    loosingFImon = currentFightingFImon;
                    break;
                }

                string commentary = "";
                Ability abilityToUse = AbilityManager.GetAbility(AttackerAbilityID);
                int damageToGive = currentFightingFImon.UseAbilityFImon(abilityToUse, waitingFightingFImon, out commentary);

                waitingFightingFImon.CurrentHealth -= damageToGive;

                var report = new DiscordEmbedBuilder()
                {
                    Title = $"Turn of: {currentFightingFImon.FImonBase.Name}",
                    Description = commentary
                };

                await ctx.Channel.SendMessageAsync(embed: report);

                if (waitingFightingFImon.CurrentHealth <= 0)
                {
                    Console.WriteLine("Oponent Died");
                    battleResult = $"{currentFightingFImon.FImonBase.Name} defeated {waitingFightingFImon.FImonBase.Name} in combat";
                    winningFImon = currentFightingFImon;
                    loosingFImon = waitingFightingFImon;
                    break;
                }

                helpFimon = currentFightingFImon;
                currentFightingFImon = waitingFightingFImon;
                waitingFightingFImon = helpFimon;
                helpFimon = null;

                helpUser = currentFightingUser;
                currentFightingUser = waitingFightingUser;
                waitingFightingUser = helpUser;
                helpUser = null;
            }

            int looserExp = random.Next(10, 20);
            int winnerExp = random.Next(30, 50);

            int winningExpReward = winningFImon.FImonBase.AwardExperience(winnerExp);
            int loosingExpReward = loosingFImon.FImonBase.AwardExperience(looserExp);
            var winningEmbed = new DiscordEmbedBuilder()
            {
                Title = $"And the winner is: {winningFImon.FImonBase.Name}",
                Color = DiscordColor.Gold,
                Description = $"The winner received {winningExpReward} exp and the looser receives {loosingExpReward} exp\n" + battleResult
            };
            await ctx.Channel.SendMessageAsync(embed: winningEmbed);
        }

        [Command("getfimon")]
        public async Task GetFimon(CommandContext ctx)
        {
            Trainer trainer = TrainerManager.GetTrainer(ctx.User.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User,ctx.Channel,ctx.Client);
            
            if (selectedFImon == null)
            {
                return;
            } 

            var FImonEmbed = GenerateFImonEmbed(trainer, new InCombatFImon(selectedFImon), true);
            await ctx.Channel.SendMessageAsync(embed: FImonEmbed).ConfigureAwait(false);            
        }

        private static Dictionary<DiscordEmoji, ReactionStepData> GenerateFImonOptions(DiscordClient discordClient, Trainer trainer)
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

        private DiscordEmbedBuilder GenerateFImonEmbed(Trainer trainer, InCombatFImon currentFightingFImon, bool displayAttacks)
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

        private Dictionary<DiscordEmoji, ReactionStepData> GenerateAttackOptions(CommandContext ctx, InCombatFImon currentFImon)
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
            if (FImonBase.SpecialAttack != null && currentFImon.HaveEnoughEnergyForAbility(specialAttack) )
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
    }
}
