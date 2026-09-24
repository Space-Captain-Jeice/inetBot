using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tesseract;

namespace InetBot.Modules
{
    internal class ScamDetector
    {
        HttpClient webClient = new HttpClient();
        Commands commands = new Commands();

        int index = 1;

        public void ScanImage(SocketMessage message, SocketGuild guild, DiscordSocketClient client)
        {
            string filename = DateTime.Now.Ticks.ToString() + ".jpg";

            if (message.Attachments.FirstOrDefault().Filename != "image.jpg") return;

            string foundText;

            try
            {

                Stream fileStream = webClient.GetStreamAsync(message.Attachments.FirstOrDefault().Url).Result;
                using (FileStream outputFileStream = new FileStream(filename, FileMode.Create))
                {
                    fileStream.CopyTo(outputFileStream);
                }

                string? tessdata_prefix = Environment.GetEnvironmentVariable("TESSDATA_PREFIX");
                if (tessdata_prefix == null) tessdata_prefix = "./tessdata";

                using (var engine = new TesseractEngine(tessdata_prefix, "eng", EngineMode.Default))
                using (var img = Pix.LoadFromFile(filename))
                using (var page = engine.Process(img))
                {
                    foundText = page.GetText();
                }

                if (foundText.ToLower().Contains("i am pleased to announce the launch of my own cryptocurrency"))
                {
                    Console.WriteLine($"Deleting message No. {index} from @{message.Author.Username} [{message.Author.Id}] for being a Mr. Beast scam.");
                    message.DeleteAsync();

                    index++;
                    if (index == 4)
                    {
                        Console.WriteLine($"Ding ding ding! Banning @{message.Author.Username} [{message.Author.Id}] for being a Mr. Beast scam.");
                        ScammerBanner(message.Author, guild, client);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally 
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }

        }

        async Task ScammerBanner(SocketUser user, SocketGuild guild, DiscordSocketClient client)
        {
            commands._user = client.CurrentUser;
            commands.isSlashCommand = false;
            commands._modChannel = guild.GetTextChannel(commands.modChannelID);
            commands._message = null;

            await commands.HandleBanCommand(guild.GetUser(user.Id), "Compromised account. (Auto)", guild);
            index = 1;
        }

    }
}
