using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class UserConnection
    {
        public HubConnection HubConnection { get; set; }
        public IHubProxy PasswordGenerator { get; set; }
    }
}