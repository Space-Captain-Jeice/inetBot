using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetBot.Modules
{
    internal class Buttons
    {
        public async Task PunishmentNextButton(SocketMessageComponent component)
        {
            var embedBuilder = new EmbedBuilder();



            await component.RespondAsync("Not implemeted");
        }
    }
}
