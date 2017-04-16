using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class CacheSettings
    {
        public int ItemLifeTimeMilliseconds { get; set; }
        public int SizeLimit { get; set; }
        public int RemovePersentage { get; set; }
        public int CheckPeriod { get; set; }
    }
}