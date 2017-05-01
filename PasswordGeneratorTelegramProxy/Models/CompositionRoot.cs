using Autofac;
using Autofac.Integration.WebApi;
using PasswordGeneratorTelegramProxy.Models;
using PasswordGeneratorTelegramProxy.Models.Configuration;
using System.Reflection;
using System.Web.Http;
using PasswordGeneratorTelegramProxy.Controllers;
using Telegram.Bot;
using Autofac.Core;
using System.Configuration;
using PasswordGeneratorTelegramProxy.Models.Cache;
using PasswordGeneratorTelegramProxy.Models.HubProxy;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class CompositionRoot
    {
        public static void ConfigureContainer()
        {
            // получаем экземпляр контейнера
            var builder = new ContainerBuilder();

            // регистрируем контроллер в текущей сборке
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // регистрация                    

            var timerPeriod = int.Parse(ConfigurationManager.AppSettings["TimerPeriod"]);
			builder.RegisterType<StandartTimerWrap>().As<IElapsed>()
				.WithParameter("interval", timerPeriod)
                .SingleInstance();

			var cacheTimeout = int.Parse(ConfigurationManager.AppSettings["CacheTimeout"]);
			var cacheSize = int.Parse(ConfigurationManager.AppSettings["CacheSize"]);
			builder.RegisterType<Cache<long, IHubProxy>>().As<ICache<long, IHubProxy>>()
				.WithParameters(new[] {
					new NamedParameter("cacheTimeout", cacheTimeout),
					new NamedParameter("cacheSize", cacheSize),
				})
                .PropertiesAutowired()
                .SingleInstance();
			            
			var telegramBotToken = ConfigurationManager.AppSettings["TelegramBotToken"];
			builder.RegisterType<TelegramBotClient>().As<ITelegramBotClient>()
                 .WithParameter("token", telegramBotToken)
                 .SingleInstance();

			builder.RegisterType<HubProxyFactory>().As<IHubProxyFactory>().SingleInstance();

			var url = ConfigurationManager.AppSettings["FacadeUrl"];
			var hubName = ConfigurationManager.AppSettings["PasswordGeneratorHub"];
			builder.Register(c => new TelegramController(
                c.Resolve<ITelegramBotClient>(),
                c.Resolve<IHubProxyFactory>(),
				url,
				hubName,
                c.Resolve<ICache<long, IHubProxy>>()));                

            // создаем новый контейнер с теми зависимостями, которые определены выше
            var container = builder.Build();

            // Get your HttpConfiguration.
            var config = GlobalConfiguration.Configuration;

            // установка сопоставителя зависимостей
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}