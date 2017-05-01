using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models.Configuration.SignalR
{
    public interface ISignalRManager
    {
        HubConnection CreateHubConnection(string url);
        string HubName { get; set; }
        IHubProxy CreateHubProxy(HubConnection connection);
    }
}
