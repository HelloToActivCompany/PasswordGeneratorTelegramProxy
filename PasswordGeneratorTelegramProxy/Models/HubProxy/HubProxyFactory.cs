using System;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models.HubProxy
{
	public class HubProxyFactory : IHubProxyFactory
	{
		public IHubProxy Create(string url, string hubName, string funcName, Action<string> func)
		{
			var connection = new HubConnection(url);
			var proxy = connection.CreateHubProxy(hubName);
			
			//
			proxy.On(funcName, func);
			//

			connection.Start().Wait();

			return proxy;
		}
	}
}