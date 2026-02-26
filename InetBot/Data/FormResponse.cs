using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InetBot.Data
{
    internal class FormResponse
    {
        [JsonProperty("Have you read and understand the rules?")]
        public string rules { get; set; }

        [JsonProperty("What is your discord username?")]
        public string username { get; set; }

        [JsonProperty("What is your punishment ID?")]
        public string id { get; set; }

        [JsonProperty("Can you briefly explain what you did to get banned?")]
        public string reasonBan { get; set; }

        [JsonProperty("Why should we unban you?")]
        public string reasonUnban { get; set; }

        [JsonProperty("Do you have an email address we can contact you at?")]
        public string email { get; set; }
    }
}
