using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR.Client;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class Cache : ICache
    {
        private Timer _timer;
        private ConcurrentDictionary<long, DictionaryItem<HubConnection>> _internalMap;

        private TimeSpan _lifeTime;
        private ITimeManager _timeManager;
        private int _sizeLimit;
        private int _removePersentage;

        public Cache(TimeSpan itemLifeTime, int sizeLimit, int removePersentage, int checkPeriod, ITimeManager timeManager)
        {
            _internalMap = new ConcurrentDictionary<long, DictionaryItem<HubConnection>>();

            _timer = new Timer((n) => Collect(), null, 1000, checkPeriod);

            _lifeTime = itemLifeTime;
            _timeManager = timeManager;
            _sizeLimit = sizeLimit;
            _removePersentage = removePersentage;
        }

        public void Add(long key, HubConnection value)
        {      
            _internalMap.AddOrUpdate(
                key, 
                new DictionaryItem<HubConnection>(value, _lifeTime, _timeManager), 
                (currentKey, oldValue) => new DictionaryItem<HubConnection>(value, _lifeTime, _timeManager));

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
            DictionaryItem<HubConnection> forRemove;
            _internalMap.TryRemove(key, out forRemove);
        }

        public bool TryGet(long key, out HubConnection connection)
        {
            DictionaryItem<HubConnection> dictItem;
            bool result = _internalMap.TryGetValue(key, out dictItem);

            if (result)
            {
                connection = dictItem.Value;

                var updatedDictItem = (DictionaryItem<HubConnection>)dictItem.Clone();
                updatedDictItem.LastChange = _timeManager.Now;

                _internalMap.TryUpdate(
                    key,
                    updatedDictItem,
                    dictItem);
            }
            else
            {
                connection = null;
            }

            return result;
        }

        private void Collect()
        {
            foreach (var pair in _internalMap)
            {
                if (pair.Value.IsObsolet())
                {
                    DictionaryItem<HubConnection> removed;
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
                DictionaryItem<HubConnection> removed;
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