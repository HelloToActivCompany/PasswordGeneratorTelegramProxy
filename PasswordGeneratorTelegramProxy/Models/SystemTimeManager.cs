using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class SystemTimeManager : ITimeManager
    {
        public DateTime Now
        {
            get { return DateTime.Now; }              
        }
    }
}