using System;
using Microsoft.AspNet.SignalR.Client;


namespace PasswordGeneratorTelegramProxy.Models.HubProxy
{
	public interface IHubProxyFactory
	{
		IHubProxy Create(string url, string hubName, string funcName, Action<string> func);
	}
}
