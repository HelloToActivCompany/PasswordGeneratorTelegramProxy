using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR.Client;
using System.Threading;
using System.Linq;
using PasswordGeneratorTelegramProxy.Models.Configuration;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class Cache : ICache
    {
        private Timer _timer;
        private ConcurrentDictionary<long, DictionaryItem<Tuple<HubConnection, IHubProxy>>> _internalMap;

        private TimeSpan _lifeTime;
        private ITimeManager _timeManager;
        private int _sizeLimit;
        private int _removePersentage;

        public Cache(IGetCacheSettings cacheManager, ITimeManager timeManager)
        {
            var cacheSettings = cacheManager.GetCacheSettings();

            _internalMap = new ConcurrentDictionary<long, DictionaryItem<Tuple<HubConnection, IHubProxy>>>();

            _timer = new Timer((n) => Collect(), null, cacheSettings.CheckPeriod, cacheSettings.CheckPeriod);
            _lifeTime = new TimeSpan(0, 0, 0, 0, cacheSettings.ItemLifeTimeMilliseconds);
            _timeManager = timeManager;
            _sizeLimit = cacheSettings.SizeLimit;
            _removePersentage = cacheSettings.RemovePersentage;
        }

        public void Add(long key, HubConnection connection, IHubProxy proxy)
        {      
            _internalMap.AddOrUpdate(
                key, 
                new DictionaryItem<Tuple<HubConnection, IHubProxy>>(new Tuple<HubConnection, IHubProxy>(connection, proxy), _lifeTime, _timeManager), 
                (currentKey, oldValue) => new DictionaryItem<Tuple<HubConnection, IHubProxy>>(new Tuple<HubConnection, IHubProxy>(connection, proxy), _lifeTime, _timeManager));

            var size = _internalMap.Count;
            if (size > _sizeLimit)
            {
                Thread collectThread = new Thread(() => CollectOldest());
                collectThread.IsBackground = false;
                collectThread.Start();
            }
        }

        public void Delete(long key)
        {
            DictionaryItem<Tuple<HubConnection, IHubProxy>> forRemove;
            _internalMap.TryRemove(key, out forRemove);
        }

        public bool TryGet(long key, out HubConnection connection, out IHubProxy proxy)
        {
            DictionaryItem<Tuple<HubConnection, IHubProxy>> dictItem;
            bool result = _internalMap.TryGetValue(key, out dictItem);

            if (result)
            {
                connection = dictItem.Value.Item1;
                proxy = dictItem.Value.Item2;

                var updatedDictItem = (DictionaryItem<Tuple<HubConnection, IHubProxy>>)dictItem.Clone();
                updatedDictItem.LastChange = _timeManager.Now;

                _internalMap.TryUpdate(
                    key,
                    updatedDictItem,
                    dictItem);
            }
            else
            {
                connection = null;
                proxy = null;
            }

            return result;
        }

        private void Collect()
        {
            foreach (var pair in _internalMap)
            {
                if (pair.Value.IsObsolet())
                {
                    DictionaryItem<Tuple<HubConnection, IHubProxy>> removed;
                    _internalMap.TryRemove(pair.Key, out removed);                    
                }
            }
        }

        private void CollectOldest()
        {
            int size = _internalMap.Count;
            var sorted = _internalMap.ToList();

            sorted.Sort((a, b) => a.Value.LastChange.CompareTo(b.Value.LastChange));

            int removeCount = _internalMap.Count * _removePersentage / 100;

            for (int i=0; i<=removeCount; i++)
            {
                DictionaryItem<Tuple<HubConnection, IHubProxy>> removed;
                _internalMap.TryRemove(sorted[i].Key, out removed);
            }
        }

        private class DictionaryItem<T> : ICloneable
        {
            public DateTime LastChange { get; set; }
            private TimeSpan _lifeTime;
            private ITimeManager _timeManager;

            public T Value { get; set; }

            public DictionaryItem(T item, TimeSpan lifeTime, ITimeManager timeManager)
            {
                Value = item;                             
                _lifeTime = lifeTime;
                _timeManager = timeManager;
                LastChange = _timeManager.Now;
            }

            public bool IsObsolet()
            {
                return _timeManager.Now - LastChange > _lifeTime;
            }

            public object Clone()
            {
                return new DictionaryItem<T>(Value, _lifeTime, _timeManager);
            }
        }
    }
}