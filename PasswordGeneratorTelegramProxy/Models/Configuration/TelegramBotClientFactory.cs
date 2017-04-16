using Telegram.Bot;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class TelegramBotClientFactory : ITelegramBotClientFactory
    {
        private ITelegramBotClient _client;

        public TelegramBotClientFactory(IGetBotToken tokenManager)
        {
            _client = new TelegramBotClient(tokenManager.GetToken());            
        }

        public ITelegramBotClient GetTelegramBotClient()
        {
            return _client;
        }
    }
}