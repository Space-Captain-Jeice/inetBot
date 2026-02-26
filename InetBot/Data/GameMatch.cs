using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InetBot.Data
{
    public class GameMatch
    {
        public int id {  get; set; }

        public Game type { get; set; }

        public List<MatchPlayer> players { get; set; }

        public enum Game : byte
        {
            MK7 = 1
        }

        public GameMatch(int ID, Game Type, List<MatchPlayer> Players)
        {
            id = ID;
            type = Type;
            players = Players;
        }
    }

    public class MatchPlayer
    {
        public ulong userId { get; set; }

        public int score { get; set; }

        public MatchPlayer(ulong UserId, int Score)
        {
            userId = UserId;
            score = Score;
        }

    }

    public class GameMatchFileRoot
    {
        public List<GameMatch> GameMatchList;
        public int GameMatchIndex;

        public static GameMatchFileRoot GetMatches()
        {
            GameMatchFileRoot GameMatchFileRoot = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GameMatchFileRoot = JsonConvert.DeserializeObject<GameMatchFileRoot>(File.ReadAllText(string.Concat(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "\\gamematches.json")));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                GameMatchFileRoot = JsonConvert.DeserializeObject<GameMatchFileRoot>(File.ReadAllText("/home/vendell/inet/gamematches.json"));
            }


            return GameMatchFileRoot;
        }
    }
}
