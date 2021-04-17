using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace FImonBotDiscord.Game
{
    public class ActionsManager
    {
        private static HashSet<ulong> inActionUsers = new HashSet<ulong>();
        private static readonly object guildLock = new object();
        public static bool SetUserInAction(ulong userID)
        {
            lock (guildLock)
            {
                if (!inActionUsers.Contains(userID))
                {
                    inActionUsers.Add(userID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool RemoveUserFromAction(ulong userID)
        {
            lock (guildLock)
            {
                if (inActionUsers.Contains(userID))
                {
                    inActionUsers.Remove(userID);
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

        public static IEnumerable<ulong> GetInActionUsers()
        {
            return inActionUsers;
        }  
    }
}
