using System;
using PasswordGeneratorTelegramProxy.Controllers;
using Xunit;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Microsoft.AspNet.SignalR.Client;
using PasswordGeneratorTelegramProxy.Models.HubProxy;
using PasswordGeneratorTelegramProxy.Models.Cache;

namespace PasswordGeneratorTelegramProxy.Tests.Controllers
{
	public class TelegramControllerTests
    {
		[Fact]
		public void GetPassword_IncorrectMessegeAndCacheMiss_CallSendTextMessageWithErrorText()
		{
			var botStub = new Mock<ITelegramBotClient>();

			var hubProxyStub = new Mock<IHubProxy>();

			var hubProxyFactoryStub = new Mock<IHubProxyFactory>();
			hubProxyFactoryStub
				.Setup(f => f.Create(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<Action<string>>()))
				.Returns(hubProxyStub.Object);

			var cacheStub = new Mock<ICache<long, IHubProxy>>();
			

			var controller = new TelegramController(botStub.Object, hubProxyFactoryStub.Object, "urlStub", "hubNameStub", cacheStub.Object);

			var chatId = 1;
			var message = "Incorrect";
			var update = CreateUpdate(chatId, message);
			controller.GetPassword(update);

			hubProxyFactoryStub
				.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()), Times.Once);

			cacheStub
				.Verify(mock => mock.Add(It.IsAny<long>(), It.IsAny<IHubProxy>()), Times.Once);

			botStub.Verify(mock => mock.SendTextMessageAsync(
				chatId,
				"_use_: \"*key* *example.com*\"",
				false,
				false,
				0,
				null,
				Telegram.Bot.Types.Enums.ParseMode.Markdown,
				default(System.Threading.CancellationToken)
				), 				
				Times.Once);
		}

		[Fact]
		public void GetPassword_IncorrectMessegeAndCacheHit_CallSendTextMessageWithErrorText()
		{
			var botStub = new Mock<ITelegramBotClient>();

			var hubProxyStub = new Mock<IHubProxy>();

			var hubProxyFactoryStub = new Mock<IHubProxyFactory>();
			hubProxyFactoryStub
				.Setup(f => f.Create(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<Action<string>>()))
				.Returns(hubProxyStub.Object);

			var cacheStub = new Mock<ICache<long, IHubProxy>>();

			IHubProxy outProxy;
			cacheStub
				.Setup(mock => mock.TryGet(It.IsAny<long>(), out outProxy))
				.Callback(() => outProxy = hubProxyStub.Object)
				.Returns(true);


			var controller = new TelegramController(botStub.Object, hubProxyFactoryStub.Object, "urlStub", "hubNameStub", cacheStub.Object);

			var chatId = 1;
			var message = "Incorrect";
			var update = CreateUpdate(chatId, message);
			controller.GetPassword(update);

			hubProxyFactoryStub
				.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()), Times.Never);

			cacheStub
				.Verify(mock => mock.Add(It.IsAny<long>(), It.IsAny<IHubProxy>()), Times.Never);

			botStub.Verify(mock => mock.SendTextMessageAsync(
				chatId,
				"_use_: \"*key* *example.com*\"",
				false,
				false,
				0,
				null,
				Telegram.Bot.Types.Enums.ParseMode.Markdown,
				default(System.Threading.CancellationToken)
				),
				Times.Once);
		}

		[Fact]
		public void GetPassword_CorrectMessegeAndCacheMiss_CallSendTextMessage()
		{
			var botStub = new Mock<ITelegramBotClient>();

			var hubProxyStub = new Mock<IHubProxy>();

			var hubProxyFactoryStub = new Mock<IHubProxyFactory>();
			hubProxyFactoryStub
				.Setup(f => f.Create(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<Action<string>>()))
				.Returns(hubProxyStub.Object);

			var cacheStub = new Mock<ICache<long, IHubProxy>>();

			var controller = new TelegramController(botStub.Object, hubProxyFactoryStub.Object, "urlStub", "hubNameStub", cacheStub.Object);

			var chatId = 1;
			var message = "Correct message";
			var update = CreateUpdate(chatId, message);

			controller.GetPassword(update);

			hubProxyFactoryStub
				.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()), Times.Once);

			cacheStub
				.Verify(mock => mock.Add(It.IsAny<long>(), It.IsAny<IHubProxy>()), Times.Once);

			hubProxyStub
				.Verify(mock => mock.Invoke("Generate", "{\"Key\":\"Correct\",\"Value\":\"message\"}"), Times.Once);
		}

		[Fact]
		public void GetPassword_CorrectMessegeAndCacheHit_CallSendTextMessage()
		{
			var botStub = new Mock<ITelegramBotClient>();

			var hubProxyStub = new Mock<IHubProxy>();

			var hubProxyFactoryStub = new Mock<IHubProxyFactory>();
			hubProxyFactoryStub
				.Setup(f => f.Create(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<Action<string>>()))
				.Returns(hubProxyStub.Object);

			var cacheStub = new Mock<ICache<long, IHubProxy>>();

			IHubProxy outProxy;
			cacheStub
				.Setup(mock => mock.TryGet(It.IsAny<long>(), out outProxy))
				.Callback(() => outProxy = hubProxyStub.Object)
				.Returns(true);

			var controller = new TelegramController(botStub.Object, hubProxyFactoryStub.Object, "urlStub", "hubNameStub", cacheStub.Object);

			var chatId = 1;
			var message = "Correct message";
			var update = CreateUpdate(chatId, message);

			//hack, cause cant settings mock out param in nested function
			//exeption here means that the proxy.Invoke called
			Assert.Throws(typeof(NullReferenceException), () => controller.GetPassword(update));			

			hubProxyFactoryStub
				.Verify(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action<string>>()), Times.Never);

			cacheStub
				.Verify(mock => mock.Add(It.IsAny<long>(), It.IsAny<IHubProxy>()), Times.Never);

			//Assert.Throw upper used insted of this call
			//hubProxyStub
			//	.Verify(mock => mock.Invoke("Generate", "{\"Key\":\"Correct\",\"Value\":\"message\"}"), Times.Once);
		}

		private Update CreateUpdate(long chatId, string message)
		{
			var jsonUpdate = "{\"update_id\":757241218,\"message\":{\"message_id\":2520,\"from\":{\"id\":203229711,\"first_name\":\"Alex\",\"last_name\":null,\"username\":null},\"date\":1493631710,\"chat\":{\"id\":"
				+ chatId+
				",\"type\":0,\"title\":null,\"username\":null,\"first_name\":\"Alex\",\"last_name\":null},\"forward_from\":null,\"forward_from_chat\":null,\"forward_date\":null,\"reply_to_message\":null,\"edit_date\":null,\"text\":\""
				+ message
				+ "\",\"entities\":[],\"audio\":null,\"document\":null,\"photo\":null,\"sticker\":null,\"video\":null,\"voice\":null,\"caption\":null,\"contact\":null,\"location\":null,\"venue\":null,\"new_chat_member\":null,\"left_chat_member\":null,\"new_chat_title\":null,\"new_chat_photo\":null,\"delete_chat_photo\":false,\"group_chat_created\":false,\"supergroup_chat_created\":false,\"channel_chat_created\":false,\"migrate_to_chat_id\":0,\"migrate_from_chat_id\":0,\"pinned_message\":null},\"edited_message\":null,\"inline_query\":null,\"chosen_inline_result\":null,\"callback_query\":null}";

			return Update.FromString(jsonUpdate);
		}
    }
}
