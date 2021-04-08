using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FImonBotDiscord.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBotDiscord.Commands
{
    public class TrainerCommands : SharedHelpForCommands
    {
        [Command("testTrainer")]
        public async Task CreateTestTrainer(CommandContext ctx)
        {
            TrainerManager.AddTrainer(ctx.User.Id, "Ash Ketchum", "The greatest FImon trainer of all time", @"https://media.comicbook.com/2016/08/ash-ketchum-194535-1280x0.jpg");
        }
    }
}
