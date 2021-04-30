using FImonBot.Handlers.Dialogue;
using FImonBot.Handlers.Dialogue.Steps;
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
using FImonBot.Game;
using FImonBot.Game.FImons;
using FImonBot.Game.Abilities;
using FImonBot.Game.Combat;
using FImonBot.Game.Stats;
using FImonBot.Game.Trainers;
using DSharpPlus;
using FImonBot.CommandAttributes;

namespace FImonBot.Commands
{
    public class FImonCommands : SharedBaseForCommands
    {             

        /// <summary>
        /// Command for creating a FImon
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [Command("addfimon")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task AddFimonCommand(CommandContext ctx)
        {
            var userID = ctx.User.Id;
            var trainer = TrainerManager.GetTrainer(userID);
            if (trainer == null)
            {
                await SendErrorMessage("Mate... you dont have a trainer yet", ctx.Channel);
                return;
            }
            if (!trainer.CanAddFImon())
            {
                await SendErrorMessage("Mate... you already have max amount of FImons", ctx.Channel);
                return;
            }

            ActionsManager.SetUserInAction(ctx.Member.Id);

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

            var userChannel = await ctx.Member.CreateDmChannelAsync();
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, nameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded) 
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return; 
            }            

            FImonManager.AddFimon(trainer.TrainerID,FImonName, description, primaryType, secondaryType, strengthValue, staminaValue, inteligenceValue, luckValue,
                agilityValue, perceptionValue, abilityPowerValue);

            await SendCorrectMessage("FIMON added successfully", userChannel);
            await SendCorrectMessage($"{FImonName} {description} {primaryType.ToString()} {secondaryType.ToString()}", userChannel);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }               

        [Command("getfimon")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        public async Task GetFimon(CommandContext ctx)
        {
            Trainer trainer = TrainerManager.GetTrainer(ctx.Member.Id);
            FImon selectedFImon = await SelectYourFImon(ctx.User,ctx.Channel,ctx.Client);
            
            if (selectedFImon == null)
            {                
                return;
            } 

            var FImonEmbed = GenerateFImonEmbed(trainer, new InCombatFImon(selectedFImon), true);
            await SendCorrectMessage(FImonEmbed, ctx.Channel).ConfigureAwait(false);            
        }

        [Command("deletefimon")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task DeleteFImon(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);

            var message = ctx.Message;
            var caller = ctx.Member;

            DiscordUser userFImonToDelete;
            if (caller.Id == authorID)
            {
                if (message.MentionedUsers.Count == 0) 
                {
                    userFImonToDelete = caller; 
                }
                else
                {
                    userFImonToDelete = message.MentionedUsers.First();
                }
            }
            else
            {
                if (message.MentionedUsers.Count == 0)
                {
                    userFImonToDelete = caller;
                }
                else
                {
                    await SendErrorMessage("You can´t delete others FImons", ctx.Channel);
                    ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                    return;
                }
            }

            if (!TrainerManager.GetTrainer(userFImonToDelete.Id).HasFImon())
            {
                await SendErrorMessage("Person whos FImon you want to delete does not have any", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            var FImonToDelete = await SelectYourFImon(userFImonToDelete, ctx.Channel, ctx.Client, caller);

            if (FImonToDelete == null)
            {
                await SendCorrectMessage("No deletion occured", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            TrainerManager.DeleteTrainersFImon(userFImonToDelete.Id, FImonToDelete.FImonID);
            await SendCorrectMessage("FImon has been deleted", ctx.Channel);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [Command("increaseStats")]
        [RequireChannelNameIncludes("afk")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task IncreaseFImonStats(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);

            var trainer = TrainerManager.GetTrainer(ctx.Member.Id);
            if (trainer == null)
            {
                await SendErrorMessage("You dont have a trainer", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            var userChannel = await ctx.Member.CreateDmChannelAsync();
            var FImon = await SelectYourFImon(ctx.Member, userChannel, ctx.Client);
            if (FImon.UnspentSkillPoints == 0)
            {
                await SendErrorMessage("Selected FImon doesnt have any unspent skill-points", userChannel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }
            
            var atributesInfo = AttributesEmbedInfo();
            atributesInfo.Description = "Please select which attribute would you like to increase";       

            var options = new Dictionary<string, TextChoiceData>();

            foreach (var key in BaseStats.attibutesOptions)
            {
                options.Add(key, new TextChoiceData("- attribute option", key));
            }

            var textChoiceStep = new TextChoiceStep("Which attribute would you like to increase?",null, options);
            string answer = null;
            textChoiceStep.optionalEmbed = atributesInfo;
            textChoiceStep.OnValidResult += result =>
            {
                answer = result.OptionalData as string;
            };

            var statIncreaseStep = new IntStep("By how many point would you like to increase?", null, 0, FImon.UnspentSkillPoints);
            int statIncrease = 0;
            statIncreaseStep.OnValidResult += result =>
            {
                statIncrease = result;
            };

            textChoiceStep.SetNextStep(statIncreaseStep);

            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, textChoiceStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            FImon.IncreaseAttribute(answer.ToLower().Trim(), statIncrease);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }
    }
}
