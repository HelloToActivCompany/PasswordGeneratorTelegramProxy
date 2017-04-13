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
using PasswordGeneratorTelegramProxy.Models;

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

            long userId = update.Message.Chat.Id;

            UserConnection userConnection;
            bool connectionExist = WebApiApplication.UserIdConnectionMap.TryGetValue(userId, out userConnection);

            if (!connectionExist)
            {
                HubConnection HubConnection = new HubConnection("http://passwordgeneratorfacade.azurewebsites.net/");
                userConnection = new UserConnection
                {
                    HubConnection = HubConnection,
                    PasswordGenerator = HubConnection.CreateHubProxy("PasswordHub")
                };

                userConnection.PasswordGenerator.On<string>("passwordReady", password => bot.SendTextMessageAsync(userId, password));
                HubConnection.Start().Wait();

                WebApiApplication.UserIdConnectionIdMap.TryAdd(userId, userConnection.HubConnection.ConnectionId);
                WebApiApplication.UserIdConnectionMap.TryAdd(userId, userConnection);
            }

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
                
                var requestData = JsonConvert.SerializeObject(new
                {
                    Key = key,
                    Value = source
                });
                userConnection.PasswordGenerator.Invoke("Generate", requestData);
            }
        }

        [HttpGet]
        public OkResult Heartbeat()
        {
            return Ok();
        }
    }
}