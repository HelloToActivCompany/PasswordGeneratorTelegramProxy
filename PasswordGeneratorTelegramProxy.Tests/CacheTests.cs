using System;
using PasswordGeneratorTelegramProxy.Models;
using Xunit;
using Moq;
using PasswordGeneratorTelegramProxy.Models.Cache;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy.Tests
{
	public class CacheTests
	{
		[Fact]
		public void CacheCRUDTest_forIHubProxy()
		{
			var startTime = new DateTime(2017, 1, 1, 1, 1, 30);

			//freez time
			var timeProviderStub = new Mock<TimeProvider>();
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(startTime);

			TimeProvider.Current = timeProviderStub.Object;

			int cacheTimeOut = 1;
			var cache = new Cache<long, IHubProxy>(10, cacheTimeOut);

			//add

			long id = 1;

			var hubProxyStub = new Mock<IHubProxy>();

			var expected = hubProxyStub.Object;

			cache.Add(id, expected);

			IHubProxy actual;
			var res = cache.TryGet(id, out actual);

			Assert.Equal(res, true);
			Assert.True(ReferenceEquals(expected, actual));

			//delete
			cache.Delete(id);
			res = cache.TryGet(id, out actual);

			Assert.Equal(res, false);
		}

		[Fact]
		public void CleanObsoletItems_ForObsoletItem_CleanItem()
		{
			//arrange
			var startTime = new DateTime(2017, 1, 1, 1, 1, 30);

			var timeProviderStub = new Mock<TimeProvider>();
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(startTime);

			TimeProvider.Current = timeProviderStub.Object;

			var cacheTimeout = 1;
			var cache = new Cache<long, string>(10, cacheTimeout);

			cache.Add(1, "obsolet");

			//cacheTimeout sec later
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(startTime + TimeSpan.FromSeconds(cacheTimeout));

			//act
			cache.CleanObsoletItems();

			string actual;
			var res = cache.TryGet(1, out actual);

			//assert
			Assert.Equal(res, false);
		}

		[Fact]
		public void CleanObsoletItems_ForNotObsoletItem_Nothing()
		{
			//arrange
			var startTime = new DateTime(2017, 1, 1, 1, 1, 30);

			var timeProviderStub = new Mock<TimeProvider>();
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(startTime);

			TimeProvider.Current = timeProviderStub.Object;

			var cacheTimeout = 1;
			var cache = new Cache<long, string>(10, cacheTimeout);

			var expected = "notObsolet";
			cache.Add(1, expected);

			//act
			cache.CleanObsoletItems();

			string actual;
			var res = cache.TryGet(1, out actual);

			//assert
			Assert.Equal(res, true);
			Assert.Equal(actual, expected);
		}

		[Fact]
		public void CleanOldestItems_WhenCacheOverflow_CleaningStart()
		{
			//arrange
			var startTime = new DateTime(2017, 1, 1, 1, 1, 30);

			var timeProviderStub = new Mock<TimeProvider>();
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(startTime);

			TimeProvider.Current = timeProviderStub.Object;

			var cacheTimeout = 1;
			int cacheSize = 10;
			var cache = new Cache<long, string>(cacheSize, cacheTimeout);

			//act, make overflow
			for (int i = 0; i < cacheSize + 1; i++)
			{
				cache.Add(i, "stub");
			}

			System.Threading.Thread.Sleep(1000);

			Assert.True(cache.Count < cacheSize + 1);
		}

		[Fact(Skip = "not worked, searching the reson")]
		public void CleanObsoletItems_WhenIElapsedTick_Clean()
		{
			//arrange
			var startTime = new DateTime(2017, 1, 1, 1, 1, 30);

			var timeProviderStub = new Mock<TimeProvider>();
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(startTime);

			ManualElapsedFake elapsedFake = new ManualElapsedFake();

			int cacheTimeout = 1;
			var cache = new Cache<int, string>(10, cacheTimeout)
			{
				Timer = elapsedFake
			};
			
			int id = 1;
			cache.Add(id, "stub");

			//act
			var timeoutTime = startTime + TimeSpan.FromSeconds(cacheTimeout);
			timeProviderStub
				.SetupGet(tp => tp.Now)
				.Returns(timeoutTime);

			elapsedFake.ThrowElapsed();

			//assert
			Assert.Equal(cache.Count, 0);
		}

		private class ManualElapsedFake : IElapsed
		{
			public event EventHandler Elapsed;

			public void ThrowElapsed()
			{
				Elapsed(this, EventArgs.Empty);
			}
		}
	}
}
