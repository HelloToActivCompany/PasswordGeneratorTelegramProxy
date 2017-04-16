using System;
using System.Web.Http;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using System.Web.Http.Results;
using Microsoft.AspNet.SignalR.Client;
using PasswordGeneratorTelegramProxy.Models;
using PasswordGeneratorTelegramProxy.Models.Configuration;
using Microsoft.AspNet.SignalR.Client.Transports;
using System.Threading.Tasks;
using Telegram.Bot;

namespace PasswordGeneratorTelegramProxy.Controllers
{
    [Route("api/telegram")]
    public class TelegramController : ApiController
    {
        private ITelegramBotClient _bot;
        private ICache _cache;
        private IHubConnectionFactory _hubConnectionFactory;
        private string _hubName;

        public TelegramController(
            ITelegramBotClientFactory botFactory,
            IHubConnectionFactory hubConnectionFactory,
            IGetPasswordGeneratorHubName hubNameManager,
            ICache cache)
        {
            _bot = botFactory.GetTelegramBotClient();
            _hubConnectionFactory = hubConnectionFactory;
            _hubName = hubNameManager.GetHubName();
            _cache = cache;
        }

        [HttpPost]
        public void GetPassword([FromBody]Update update)
        {
            long userId = update.Message.Chat.Id;

            HubConnection userConnection;
            IHubProxy passwordGeneratorProxy;

            Task startConnectionTask = null;

            bool connectionExist = _cache.TryGet(userId, out userConnection, out passwordGeneratorProxy);
            if (!connectionExist)
            {
                userConnection = _hubConnectionFactory.CreateHubConnection("stub");
                passwordGeneratorProxy = userConnection.CreateHubProxy(_hubName);
                passwordGeneratorProxy.On<string>("passwordReady", password => _bot.SendTextMessageAsync(userId, password));

                _cache.Add(userId, userConnection, passwordGeneratorProxy);

                startConnectionTask = userConnection.Start();
            }

            if (!String.IsNullOrWhiteSpace(update.Message.Text))
            {
                var param = update.Message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (param.Length != 2)
                {
                    var msg = _bot.SendTextMessageAsync(
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
        public IHttpActionResult Heartbeat()
        {
            return Ok("pulse");
        }
    }
}
