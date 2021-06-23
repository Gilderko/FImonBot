using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace FImonBot.CommandAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireChannelNameIncludes : CheckBaseAttribute
    {
        private string[] stringName;

        public RequireChannelNameIncludes(params string[] name)
        {
            stringName = name;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            foreach (var possible_name in stringName)
            {
                if (ctx.Channel.Name.Contains(possible_name, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
