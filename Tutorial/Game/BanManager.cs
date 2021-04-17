using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace FImonBotDiscord.Game
{
    public class ActionManager
    {
        private static HashSet<ulong> inActionUsers = new HashSet<ulong>();
        private static readonly object guildLock = new object();
        public static bool SetUserInAction(DiscordUser user)
        {
            lock (guildLock)
            {
                if (!inActionUsers.Contains(user.Id))
                {
                    inActionUsers.Add(user.Id);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool RemoveUserFromAction(DiscordUser user)
        {
            lock (guildLock)
            {
                if (inActionUsers.Contains(user.Id))
                {
                    inActionUsers.Remove(user.Id);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsInAction(ulong userID)
        {
            bool isInActiion = false;
            lock (guildLock)
            {
                isInActiion = inActionUsers.Contains(userID);
            }
            return isInActiion;
        }

        public static IEnumerable<ulong> GetBannedUsersIDs()
        {
            return inActionUsers;
        }  
    }
}
