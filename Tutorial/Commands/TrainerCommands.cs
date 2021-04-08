using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using FImonBotDiscord.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBotDiscord.Commands
{
    public class TrainerCommands : SharedBaseForCommands
    {
        [Command("testTrainer")]
        public async Task CreateTestTrainer(CommandContext ctx)
        {
            TrainerManager.AddTrainer(ctx.User.Id, "Ash Ketchum", "The greatest FImon trainer of all time", @"https://media.comicbook.com/2016/08/ash-ketchum-194535-1280x0.jpg");
        }

        [Command("deleteTrainer")]
        public async Task DeleteTrainer(CommandContext ctx)
        {
            var message = ctx.Message;
            var caller = ctx.Member;

            DiscordUser userFImonToDelete;
            if (caller.Id == authorID)
            {
                Console.WriteLine("Author");
                if (message.MentionedUsers.Count == 0)
                {
                    Console.WriteLine("Is null");
                    userFImonToDelete = caller;
                }
                else
                {
                    Console.WriteLine("Somebody tagged");
                    userFImonToDelete = message.MentionedUsers.First();
                }
            }
            else
            {
                userFImonToDelete = caller;
            }          

            var confirmation = await ConfirmPreviousStep(ctx,caller);
            if (confirmation)
            {
                TrainerManager.DeleteTrainer(userFImonToDelete.Id);
                await SendCorrectMessage("Trainer has been deleted", ctx);
            }
        }
    }
}
