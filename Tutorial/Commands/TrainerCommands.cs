using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBot.CommandAttributes;
using FImonBotDiscord.Game;
using FImonBotDiscord.Handlers.Dialogue;
using FImonBotDiscord.Handlers.Dialogue.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBotDiscord.Commands
{
    public class TrainerCommands : SharedBaseForCommands
    {
        [Command("createTrainer")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task CreateTrainer(CommandContext ctx)
        {
            if (TrainerManager.GetTrainer(ctx.Member.Id) != null)
            {
                await SendErrorMessage("You already are registered as a trainer", ctx.Channel);
                return;
            }

            ActionsManager.SetUserInAction(ctx.Member.Id);

            var nameStep = new TextStep("Welcome \nPlease choose the name for your trainer", null, 1, 30);
            string trainerName = "";
            nameStep.OnValidResult += (result) => trainerName = result;

            var backStoryStep = new TextStep("What is your trainers backstory?", null, 1, 255);
            string backstory = "";
            backStoryStep.OnValidResult += (result) => backstory = result;

            var imageUrlStep = new TextStep("Add some snazzy image URL for your trainer", null, 1, 255);
            string imageUrl = "";
            imageUrlStep.OnValidResult += (result) => imageUrl = result;

            nameStep.SetNextStep(backStoryStep);
            backStoryStep.SetNextStep(imageUrlStep);
            imageUrlStep.SetNextStep(null);

            var userChannel = await ctx.Member.CreateDmChannelAsync();
            var inputDialogueHandler = new DialogueHandler(ctx.Client, userChannel, ctx.User, nameStep);

            bool succeeded = await inputDialogueHandler.ProcessDialogue().ConfigureAwait(false);

            if (!succeeded)
            {
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }

            TrainerManager.AddTrainer(ctx.User.Id, trainerName, backstory, imageUrl);
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }

        [RequireNotBanned]
        [Command("getTrainer")]        
        public async Task GetTrainer(CommandContext ctx)
        {

            var trainer = TrainerManager.GetTrainer(ctx.Member.Id);
            if (trainer == null)
            {
                await SendErrorMessage("You are not registered as a FImon trainer", ctx.Channel);
                return;
            }

            var trainerEmbed = new DiscordEmbedBuilder()
            {
                Title = trainer.Name,
                Color = DiscordColor.Gold,
                ImageUrl = trainer.ImageUrl
            };
            trainerEmbed.AddField("Battle history", $"{trainer.BattlesWon} wins vs {trainer.BattlesLost} loses");
            if (trainer.FImon1ID != null) { trainerEmbed.AddField(trainer.FImon1.Name,$"{trainer.FImon1.PrimaryType} {trainer.FImon1.SecondaryType} "); }
            if (trainer.FImon2ID != null) { trainerEmbed.AddField(trainer.FImon2.Name, $"{trainer.FImon2.PrimaryType} {trainer.FImon2.SecondaryType} "); }
            if (trainer.FImon3ID != null) { trainerEmbed.AddField(trainer.FImon3.Name, $"{trainer.FImon3.PrimaryType} {trainer.FImon3.SecondaryType} "); }
            if (trainer.FImon4ID != null) { trainerEmbed.AddField(trainer.FImon4.Name, $"{trainer.FImon4.PrimaryType} {trainer.FImon4.SecondaryType} "); }

            trainerEmbed.AddField("Backstory", trainer.Backstory);

            await SendCorrectMessage(trainerEmbed, ctx.Channel);
        }

        [Command("deleteTrainer")]
        [RequireNotBanned]
        [RequireNotInAction]
        public async Task DeleteTrainer(CommandContext ctx)
        {
            ActionsManager.SetUserInAction(ctx.Member.Id);

            var message = ctx.Message;
            var caller = ctx.Member;

            DiscordUser userTrainerToDelete;
            if (caller.Id == authorID)
            {
                Console.WriteLine("Author");
                if (message.MentionedUsers.Count == 0)
                {
                    Console.WriteLine("Is null");
                    userTrainerToDelete = caller;
                }
                else
                {
                    Console.WriteLine("Somebody tagged");
                    userTrainerToDelete = message.MentionedUsers.First();
                }
            }
            else
            {
                userTrainerToDelete = caller;
            } 
            
            if (TrainerManager.GetTrainer(userTrainerToDelete.Id) == null)
            {
                await SendErrorMessage("Given user does not have a trainer", ctx.Channel);
                ActionsManager.RemoveUserFromAction(ctx.Member.Id);
                return;
            }


            var confirmation = await ConfirmPreviousStep(ctx,caller);
            if (confirmation)
            {
                TrainerManager.DeleteTrainer(userTrainerToDelete.Id);
                await SendCorrectMessage("Trainer has been deleted", ctx.Channel);
            }
            else
            {
                await SendCorrectMessage("No trainer has been deleted", ctx.Channel);
            }
            ActionsManager.RemoveUserFromAction(ctx.Member.Id);
        }
    }
}
