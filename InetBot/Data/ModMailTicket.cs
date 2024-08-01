using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InetBot.Data
{
    public class ModMailTicket
    {
        public ulong channelID { get; set; }

        public ulong userID { get; set; }

        public ulong ticketID { get; set; }

        public ulong? punishmentID { get; set; }

        public bool isOpen { get; set; }

        public ulong closingModID { get; set; }

        public string? closingReason { get; set; }

        public List<ModMailMessage>? associatedMessages { get; set; }
    }

    public class ModMailMessage
    {
        public ulong authorID { get; set; }
        public ulong messageID { get; set; }
        public string content { get; set; }
    }

    public class ModMailTicketFileRoot
    {
        public List<ModMailTicket>? ModMailTicketList;
        public ulong modmailIndex;

        public static ModMailTicketFileRoot GetModMailTickets()
        {
            ModMailTicketFileRoot modMailTicketFileRoot = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (File.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\modmailtickets.json"))
                {
                    modMailTicketFileRoot = JsonConvert.DeserializeObject<ModMailTicketFileRoot>(File.ReadAllText(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\modmailtickets.json"));
                }
                else
                {
                    modMailTicketFileRoot.modmailIndex = 0;
                    modMailTicketFileRoot.ModMailTicketList = new List<ModMailTicket>();
                    File.WriteAllText(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\modmailtickets.json", JsonConvert.SerializeObject(modMailTicketFileRoot, Formatting.Indented));
                    modMailTicketFileRoot = JsonConvert.DeserializeObject<ModMailTicketFileRoot>(File.ReadAllText(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\modmailtickets.json"));
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/home/vendell/inet/modmailtickets.json"))
                {
                    modMailTicketFileRoot = JsonConvert.DeserializeObject<ModMailTicketFileRoot>(File.ReadAllText("/home/vendell/inet/modmailtickets.json"));
                }
                else
                {
                    modMailTicketFileRoot.modmailIndex = 0;
                    modMailTicketFileRoot.ModMailTicketList = new List<ModMailTicket>();
                    File.WriteAllText("/home/vendell/inet/modmailtickets.json", JsonConvert.SerializeObject(modMailTicketFileRoot, Formatting.Indented));
                    modMailTicketFileRoot = JsonConvert.DeserializeObject<ModMailTicketFileRoot>(File.ReadAllText("/home/vendell/inet/modmailtickets.json"));
                }
            }

            return modMailTicketFileRoot;
        }
    }
}
