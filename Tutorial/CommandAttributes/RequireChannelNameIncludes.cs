﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace FImonBot.CommandAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireChannelNameIncludes : CheckBaseAttribute
    {
        public string stringName;

        public RequireChannelNameIncludes(string name)
        {
            stringName = name;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(ctx.Channel.Name.Contains(stringName, StringComparison.OrdinalIgnoreCase));
        }
    }
}