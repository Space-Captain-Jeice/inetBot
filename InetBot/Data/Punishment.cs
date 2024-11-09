using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InetBot.Data
{
    public class Punishment
    {
        public ulong targetID { get; set; }

        public Type type { get; set; }

        public string reason { get; set; }

        public string duration { get; set; }

        public ulong modID { get; set; }

        public ulong punishmentID { get; set; }

        public long timestamp { get; set; }

        public ulong? notifMsgID { get; set; }

        public bool active { get; set; }

        public enum Type : byte
        {
            WARN = 1,
            MUTE,
            KICK,
            BAN
        }
    }

    public class PunishmentFileRoot
    {
        public List<Punishment>? punishmentList;
        public ulong punishmentIndex;

        public static PunishmentFileRoot GetPunishments() 
        {
            PunishmentFileRoot punishmentFileRoot = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                punishmentFileRoot = JsonConvert.DeserializeObject<PunishmentFileRoot>(File.ReadAllText(string.Concat(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "\\punishments.json")));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                punishmentFileRoot = JsonConvert.DeserializeObject<PunishmentFileRoot>(File.ReadAllText("/home/vendell/inet/punishments.json"));
            }


            return punishmentFileRoot;
        }
    }
}
