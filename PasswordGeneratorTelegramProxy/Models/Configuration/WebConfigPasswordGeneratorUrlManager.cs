using System.Configuration;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class WebConfigPasswordGeneratorUrlManager : IGetPasswordGeneratorUrl
    {
        private string _url;

        public WebConfigPasswordGeneratorUrlManager()
        {
            _url = ConfigurationManager.AppSettings["PasswordGenerator"];
        }
        public string GetUrl()
        {
            return _url;
        }
    }
}