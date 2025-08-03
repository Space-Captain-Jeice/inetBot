using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Google.Apis;
using Google.Apis.Services;
using Google.Apis.Forms.v1;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace InetBot.Modules
{
    internal class BanAppeals
    {
        public async void CheckAppeals()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { FormsService.Scope.FormsResponsesReadonly },
                    "user", CancellationToken.None, new FileDataStore("Forms.Responses"));
            }

            var service = new FormsService(new BaseClientService.Initializer
            {
                ApplicationName = "InetBot Appeals",
                HttpClientInitializer = credential,
            });

            var result = await service.Forms.Responses.List("1y6uhubP6qf_4PqESykefnSFNjfLvB7uGscdMAgiq3-M").ExecuteAsync();

            foreach (var item in result.Responses)
            {
                Console.WriteLine(item.Answers.Last().Value.TextAnswers.Answers.First().Value);
            }
        }
    }
}
