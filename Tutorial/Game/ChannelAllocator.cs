using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FImonBot.Game
{
    public static class ChannelAllocator
    {
        private static HashSet<ulong> allocatedRooms = new HashSet<ulong>();
        private static readonly object guildLock = new object();

        public static bool AllocateRoom(ulong channelID)
        {
            lock (guildLock)
            {
                if (!allocatedRooms.Contains(channelID))
                {
                    allocatedRooms.Add(channelID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool FreeRoom(ulong channelID)
        {
            lock (guildLock)
            {
                if (allocatedRooms.Contains(channelID))
                {
                    allocatedRooms.Remove(channelID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsOccupiedChannel(ulong channelID)
        {
            bool isOccupied = false;
            lock (guildLock)
            {
                isOccupied = allocatedRooms.Contains(channelID);
            }
            return isOccupied;
        }
    }
}
