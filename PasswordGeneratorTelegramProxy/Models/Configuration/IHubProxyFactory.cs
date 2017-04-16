using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public interface IHubProxyFactory
    {
        IHubProxy GetProxy(string hubName, HubConnection connection);
    }
}
