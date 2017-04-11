using System;
using System.Web.Http;
using Telegram.Bot.Types;
using System.ServiceModel;
using System.Configuration;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;
using System.Web.Http.Results;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Controllers
{
    [Route("api/telegram")]
    public class TelegramController : ApiController
    {
        public static HubConnection HubConnection { get; set; }
        public static IHubProxy PasswordGenerator { get; set; }

        TelegramController()
        {
            
        }

        [HttpPost]
        public void GetPassword([FromBody]Update update)
        {
            var token = ConfigurationManager.AppSettings["TelegramBotToken"];
            var bot = new Telegram.Bot.TelegramBotClient(token);

            HubConnection = new HubConnection("http://passwordgeneratorfacade.azurewebsites.net/");
            var PasswordGenerator = HubConnection.CreateHubProxy("PasswordHub");
            PasswordGenerator.On<string>("passwordReady", (password) => { bot.SendTextMessageAsync(update.Message.Chat.Id, password); });
            //ServicePointManager.DefaultConnectionLimit = 10;
            HubConnection.Start().Wait();

            if (!String.IsNullOrWhiteSpace(update.Message.Text))
            {
                var param = update.Message.Text.Split();

                if (param.Length != 2)
                {
                    var msg = bot.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "_use_: \"*key* *example.com*\"",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                    return;
                }

                var key = param[0];
                var source = param[1];

                //var password = PostToAggregationService(key, source);
                var requestData = JsonConvert.SerializeObject(new
                {
                    Key = key,
                    Value = source
                });
                WebApiApplication.PasswordGenerator.Invoke("Generate", requestData).Wait();
                //bot.SendTextMessageAsync(update.Message.Chat.Id, password);
            }
        }

        private string PostToAggregationService(string key, string value)
        {
            var url = ConfigurationManager.AppSettings["AggregatorUrl"];

            string jsonResponse = string.Empty;
            using (var webClient = new WebClient())
            {
                var pars = new NameValueCollection();
                pars.Add("key", key);
                pars.Add("value", value);

                var response = webClient.UploadValues(url, pars);

                jsonResponse = Encoding.UTF8.GetString(response);
                string password = JsonConvert.DeserializeObject<string>(jsonResponse);

                return password;
            }
        }

        [HttpGet]
        public OkResult Heartbeat()
        {
            return Ok();
        }
    }
}