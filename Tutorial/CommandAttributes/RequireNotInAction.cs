using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using FImonBotDiscord.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBot.CommandAttributes
{
    class RequireNotInAction : CheckBaseAttribute
    {
        public RequireNotInAction()
        {

        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(BanManager.IsBanned(ctx.Member.Id));
        }
    }
}
