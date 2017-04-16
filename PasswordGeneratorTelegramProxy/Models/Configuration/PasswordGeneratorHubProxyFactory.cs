using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models.Configuration
{
    public class PasswordGeneratorHubProxyFactory : IHubProxyFactory
    {
        private static IHubProxy _proxy;

        public PasswordGeneratorHubProxyFactory(
            IGetPasswordGeneratorHubName hubNameManager, 
            IHubConnectionFactory connectionFactory
            )
        {
            var connection = connectionFactory.CreateHubConnection("stub");
            var hubName = hubNameManager.GetHubName();
            _proxy = connection.CreateHubProxy(hubName);
        }

        public IHubProxy GetProxy(string hubName, HubConnection connection)
        {
            return _proxy;
        }
    }
}