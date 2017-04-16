using System.Configuration;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class WebConfigCacheSettingsManager : IGetCacheSettings
    {
        private CacheSettings _cacheSettigs;

        public WebConfigCacheSettingsManager()
        {
            _cacheSettigs = new CacheSettings();
            
            _cacheSettigs.ItemLifeTimeMilliseconds = int.Parse(ConfigurationManager.AppSettings["ItemLifeTimeMilliseconds"]);
            _cacheSettigs.SizeLimit = int.Parse(ConfigurationManager.AppSettings["SizeLimit"]);
            _cacheSettigs.RemovePersentage = int.Parse(ConfigurationManager.AppSettings["RemovePersentage"]);
            _cacheSettigs.CheckPeriod = int.Parse(ConfigurationManager.AppSettings["CheckPeriod"]);
        }

        public CacheSettings GetCacheSettings()
        {
            return _cacheSettigs;
        }
    }
}