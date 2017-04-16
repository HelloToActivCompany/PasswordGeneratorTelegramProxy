using Telegram.Bot;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public interface ITelegramBotClientFactory
    {
        ITelegramBotClient GetTelegramBotClient();
    }
}
