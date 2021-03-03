using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.FimonManager
{
    static class FimonManager
    {
        public static Dictionary<ulong, string> mapping = new Dictionary<ulong, string>();
        private static bool isSaving;

        public static void SaveFimons()
        {
            string path = $"D:\\Skolaahobby\\HOBBY\\Discord\\Tutorial\\Tutorial\\SavedData\\newfile.sav";
            object state = mapping as object;
            using (FileStream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }

        public static void LoadFimons()
        {
            string path = $"D:\\Skolaahobby\\HOBBY\\Discord\\Tutorial\\Tutorial\\SavedData\\newfile.sav";
            
            if (!File.Exists(path))
            {
                mapping = new Dictionary<ulong, string>();
                return;
            }
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                mapping = formatter.Deserialize(stream) as Dictionary<ulong, string>;
                foreach (var map in mapping.Values)
                {
                    Console.WriteLine(map);
                }

            }      
        }

        public static void AddFimon(ulong userId, string message)
        {
            mapping[userId] = message;
            while (isSaving)
            {

            }
            SaveFimons();
        }
    }
}
