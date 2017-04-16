using System.Configuration;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class WebConfigPasswordGeneratorHubNameManager : IGetPasswordGeneratorHubName
    {
        private string _hubName;

        public WebConfigPasswordGeneratorHubNameManager()
        {
            _hubName = ConfigurationManager.AppSettings["PasswordGeneratorHub"];
        }

        public string GetHubName()
        {
            return _hubName;
        }
    }
}