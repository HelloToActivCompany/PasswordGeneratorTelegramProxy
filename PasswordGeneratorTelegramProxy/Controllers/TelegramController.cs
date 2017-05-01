using System;
using System.Web.Http;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client;
using PasswordGeneratorTelegramProxy.Models.Cache;
using Microsoft.AspNet.SignalR.Client.Transports;
using Telegram.Bot;
using PasswordGeneratorTelegramProxy.Models.HubProxy;

namespace PasswordGeneratorTelegramProxy.Controllers
{
    [Route("api/telegram")]
    public class TelegramController : ApiController
    {
        private ITelegramBotClient _bot;        
		private IHubProxyFactory _proxyFactory;		
		private string _url;
		private string _hubName;
		private ICache<long, IHubProxy> _cache;

		public TelegramController(
            ITelegramBotClient bot,
            IHubProxyFactory proxyFactory,
			string url,
			string hubName,
            ICache<long, IHubProxy> cache)
        {
			if (bot == null) throw new ArgumentNullException("proxyFactory");
			if (proxyFactory == null) throw new ArgumentNullException("proxyFactory");
			if (String.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
			if (String.IsNullOrWhiteSpace(hubName)) throw new ArgumentNullException("hubName");
			if (cache == null) throw new ArgumentNullException("cache");

			_bot = bot;
			_proxyFactory = proxyFactory;
			_url = url;
			_hubName = hubName;
			_cache = cache;
        }

        [HttpPost]
        public void GetPassword([FromBody]Update update)
        {
            long userId = update.Message.Chat.Id;

            IHubProxy proxy;

            bool connectionExist = _cache.TryGet(userId, out proxy);
            if (!connectionExist)
			{
				proxy = _proxyFactory.Create(_url, _hubName, "passwordReady", password => _bot.SendTextMessageAsync(userId, password));

                _cache.Add(userId, proxy);
            }

			if (!String.IsNullOrWhiteSpace(update.Message.Text))
			{
				var param = update.Message.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (param.Length == 2)
				{
					var key = param[0];
					var source = param[1];

					var requestData = JsonConvert.SerializeObject(new
					{
						Key = key,
						Value = source
					});

						proxy.Invoke("Generate", requestData);
					return;
				}
			}

			_bot.SendTextMessageAsync(
				update.Message.Chat.Id,
				"_use_: \"*key* *example.com*\"",
				parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
			return;
        }

        [HttpGet]
        public IHttpActionResult Heartbeat()
        {
            return Ok("pulse");
        }
    }
}
