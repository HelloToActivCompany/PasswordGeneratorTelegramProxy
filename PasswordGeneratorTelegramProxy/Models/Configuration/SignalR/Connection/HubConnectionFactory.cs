using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class HubConnectionFactory : IHubConnectionFactory
    {
        private string _url;

        public HubConnectionFactory(string url)
        {
			_url = url;
		}

        public HubConnection CreateHubConnection(string url)
        {
            return new HubConnection(_url);
        }
    }
}