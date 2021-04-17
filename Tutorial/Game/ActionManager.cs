using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace FImonBotDiscord.Game
{
    public class BanManager
    {
        private static HashSet<ulong> bannedUsers = new HashSet<ulong>();
        private const string fileBanName = "bans.sav";
        private static readonly object guildLock = new object();
        public static bool BanUser(DiscordUser user, out string comment)
        {
            lock (guildLock)
            {
                if (!bannedUsers.Contains(user.Id))
                {
                    comment = $"Banning {user.Username}";
                    bannedUsers.Add(user.Id);
                    SaveFile();
                    return true;
                }
                else
                {
                    comment = $"{user.Username} is already banned";
                    return false;
                }
            }
        }

        public static bool UnBanUser(DiscordUser user, out string comment)
        {
            lock (guildLock)
            {
                if (bannedUsers.Contains(user.Id))
                {
                    comment = $"Unbanning {user.Username}";
                    bannedUsers.Remove(user.Id);
                    SaveFile();
                    return true;
                }
                else
                {
                    comment = $"{user.Username} is not banned";
                    return false;
                }
            }
        }

        public static bool IsBanned(ulong userID)
        {
            bool isbanned = false;
            lock (guildLock)
            {
                isbanned = bannedUsers.Contains(userID);
            }
            return isbanned;
        }

        public static IEnumerable<ulong> GetBannedUsersIDs()
        {
            return bannedUsers;
        }

        private static void SaveFile()
        {
            object state = bannedUsers;
            using (FileStream stream = File.Open(fileBanName, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }

        public static async Task LoadFile()
        {
            if (!File.Exists(fileBanName))
            {
                return;
            }
            using (FileStream stream = File.Open(fileBanName, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                var loadedBannedUsers = (HashSet<ulong>)formatter.Deserialize(stream);
                bannedUsers = loadedBannedUsers ?? new HashSet<ulong>();
            }
        }


    }
}
