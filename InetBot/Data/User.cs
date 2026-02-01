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
    internal class User
    {
        public ulong Id { get; set; }
        public long coins { get; set; }
        public long kappas { get; set; }
        public long credits { get; set; }

        public class UserFileRoot
        {
            public List<User>? userList;

            public static UserFileRoot GetUsers()
            {
                UserFileRoot userFileRoot = new();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    userFileRoot = JsonConvert.DeserializeObject<UserFileRoot>(File.ReadAllText(string.Concat(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "\\users.json")));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    userFileRoot = JsonConvert.DeserializeObject<UserFileRoot>(File.ReadAllText("/home/vendell/inet/users.json"));
                }


                return userFileRoot;
            }
        }
    }
}
