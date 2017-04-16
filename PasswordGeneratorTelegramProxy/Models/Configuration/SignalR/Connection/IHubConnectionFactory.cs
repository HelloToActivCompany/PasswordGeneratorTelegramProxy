using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public interface IHubConnectionFactory
    {
        HubConnection CreateHubConnection(string url);
    }
}
