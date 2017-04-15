using System;
using System.Web.Http;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.Web.Http.Results;
using Microsoft.AspNet.SignalR.Client;
using PasswordGeneratorTelegramProxy.Models;
using Microsoft.AspNet.SignalR.Client.Transports;
using System.Threading.Tasks;

namespace PasswordGeneratorTelegramProxy.Controllers
{
    [Route("api/telegram")]
    public class TelegramController : ApiController
    {
        [HttpPost]
        public void GetPassword([FromBody]Update update)
        {
            var bot = WebApiApplication.BotClient;
            long userId = update.Message.Chat.Id;

            HubConnection userConnection;
            IHubProxy passwordGeneratorProxy = WebApiApplication.PasswordGeneratorProxy;

            bool connectionExist = WebApiApplication.Cache.TryGet(userId, out userConnection);

            Task startConnectionTask = null;

            if (!connectionExist)
            {
                userConnection = new HubConnection(WebApiApplication.PasswordGeneratorUrl);

                lock (WebApiApplication.Sync)
                {
                    if (WebApiApplication.PasswordGeneratorProxy != null)
                        passwordGeneratorProxy = WebApiApplication.PasswordGeneratorProxy;
                    else
                    {
                        WebApiApplication.PasswordGeneratorProxy = userConnection.CreateHubProxy("PasswordHub");
                        passwordGeneratorProxy = WebApiApplication.PasswordGeneratorProxy;
                    }
                }

                passwordGeneratorProxy.On<string>("passwordReady", password => bot.SendTextMessageAsync(userId, password));                

                startConnectionTask = userConnection.Start();

                WebApiApplication.Cache.Add(userId, userConnection);
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

                if (!connectionExist)
                    startConnectionTask.Wait();

                passwordGeneratorProxy.Invoke("Generate", requestData);
            }
        }

        [HttpGet]
        public OkResult Heartbeat()
        {
            return Ok();
        }
    }
}