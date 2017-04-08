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

namespace PasswordGeneratorTelegramProxy.Controllers
{
    [Route("api/telegram")]
    public class TelegramController : ApiController
    { 
        [HttpPost]
        public void GetPassword([FromBody]Update update)
        {
            var token = ConfigurationManager.AppSettings["TelegramBotToken"];
            var bot = new Telegram.Bot.TelegramBotClient(token);

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

                var password = PostToAggregationService(key, source);

                bot.SendTextMessageAsync(update.Message.Chat.Id, password);
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