using Autofac;
using Autofac.Integration.WebApi;
using PasswordGeneratorTelegramProxy.Models.Configuration;
using PasswordGeneratorTelegramProxy.Models;
using System.Reflection;
using System.Web.Http;
using PasswordGeneratorTelegramProxy.Controllers;

namespace PasswordGeneratorTelegramProxy.Utils
{
    public class AutofacConfig
    {
        public static void ConfigureContainer()
        {
            // получаем экземпляр контейнера
            var builder = new ContainerBuilder();

            // регистрируем контроллер в текущей сборке
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // регистрация
            builder.RegisterType<WebConfigBotTokenManager>().As<IGetBotToken>().SingleInstance();
            builder.RegisterType<WebConfigPasswordGeneratorUrlManager>().As<IGetPasswordGeneratorUrl>().SingleInstance();

            builder.RegisterType<TelegramBotClientFactory>().As<ITelegramBotClientFactory>().SingleInstance();
            builder.Register(c => new TelegramBotClientFactory(c.Resolve<IGetBotToken>()));

            builder.RegisterType<WebConfigCacheSettingsManager>().As<IGetCacheSettings>().SingleInstance();
            builder.RegisterType<SystemTimeManager>().As<ITimeManager>().SingleInstance();

            builder.RegisterType<Cache>().As<ICache>().SingleInstance();
            builder.Register(c => new Cache(c.Resolve<IGetCacheSettings>(), c.Resolve<ITimeManager>()));

            builder.RegisterType<WebConfigPasswordGeneratorHubNameManager>().As<IGetPasswordGeneratorHubName>().SingleInstance();
            builder.RegisterType<HubConnectionFactory>().As<IHubConnectionFactory>();

            builder.RegisterType<PasswordGeneratorHubProxyFactory>().As<IHubProxyFactory>().SingleInstance();

            builder.Register(c => new TelegramController(
                c.Resolve<ITelegramBotClientFactory>(), 
                c.Resolve<IHubConnectionFactory>(),
                c.Resolve<IGetPasswordGeneratorHubName>(), 
                c.Resolve<ICache>()));                

            // создаем новый контейнер с теми зависимостями, которые определены выше
            var container = builder.Build();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // установка сопоставителя зависимостей
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}