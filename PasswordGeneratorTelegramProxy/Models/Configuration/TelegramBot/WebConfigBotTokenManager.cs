using System.Configuration;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class WebConfigBotTokenManager : IGetBotToken
    {
        private string _token;

        public WebConfigBotTokenManager()
        {
            _token = ConfigurationManager.AppSettings["TelegramBotToken"];
        }

        public string GetToken()
        {            
            return _token;
        }
    }
}