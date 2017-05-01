using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace PasswordGeneratorTelegramProxy.Models.Cache
{
    public class Cache<TKey, TValue> : ICache<TKey, TValue>
    {       
        private ConcurrentDictionary<TKey, CacheEntry<TValue>> _internalMap;

        private TimeSpan _cacheTimeout;
        private int _cacheSize;

		private int _persentageForDropOnOverflow;
		public int PersentageForDropOnOverflow
		{
			get
			{
				if (_persentageForDropOnOverflow == 0) _persentageForDropOnOverflow = 25;
				return _persentageForDropOnOverflow;
			}

			set
			{
				if (value <= 0 || value > 100)
				{
					throw new ArgumentException("value % must be between 1 and 100");
				}

				_persentageForDropOnOverflow = value;
			}
		}
		
		#region Timer
		private IElapsed _timer;
        public IElapsed Timer
        {
            get { return _timer; }

            set
            {
                _timer = value;

                if (_timer != null)
                {
                    _timer.Elapsed += TimerElapsed;
                }
            }
        }

		private void TimerElapsed(object sender, EventArgs e)
		{
			CleanObsoletItems();
		}
		#endregion

		public Cache(int cacheSize, int cacheTimeout)
        {            
            _internalMap = new ConcurrentDictionary<TKey, CacheEntry<TValue>>();
			
			_cacheTimeout = TimeSpan.FromSeconds(cacheTimeout);
            _cacheSize = cacheSize;
        }
       

        public void Add(TKey key, TValue value)
        {
			var expiration = TimeProvider.Current.Now + _cacheTimeout;

            _internalMap.AddOrUpdate(
                key, 
                new CacheEntry<TValue>(value, expiration), 
                (currentKey, oldValue) => new CacheEntry<TValue>(value, expiration));

            var size = _internalMap.Count;
            if (size > _cacheSize)
            {
                Thread collectThread = new Thread(() => CleanOldestItems(PersentageForDropOnOverflow))
                {
                    IsBackground = false
                };
                collectThread.Start();
            }
        }

        public void Delete(TKey key)
        {
            CacheEntry<TValue> forRemove;
            _internalMap.TryRemove(key, out forRemove);
        }

        public bool TryGet(TKey key, out TValue value)
        {
            CacheEntry<TValue> cacheEntry;
            bool result = _internalMap.TryGetValue(key, out cacheEntry);

            if (result)
            {
                value = cacheEntry.Value;

				var newExpiration = TimeProvider.Current.Now + _cacheTimeout;
				var cacheEntryForUpdate = new CacheEntry<TValue>(value, newExpiration);

                _internalMap.TryUpdate(
                    key,
                    cacheEntryForUpdate,
                    cacheEntry);
            }
            else
            {
                value = default(TValue);
            }

            return result;
        }

		public int Count => _internalMap.Count;

		public void CleanObsoletItems()
        {
            foreach (var pair in _internalMap)
            {
                if (pair.Value.IsObsolet())
                {
                    CacheEntry<TValue> forRemove;
                    _internalMap.TryRemove(pair.Key, out forRemove);
                }
            }
        }

        private void CleanOldestItems(int persentageForDrop)
        {

			if (persentageForDrop < 0 || persentageForDrop > 100)
			{
				throw new ArgumentException("wrong persentageForDrop");
			}

            var sorted = _internalMap.ToList();

            sorted.Sort((a, b) => a.Value._expiration.CompareTo(b.Value._expiration));

            int removeCount = _cacheSize * persentageForDrop / 100;

            for (int i=0; i<removeCount; i++)
            {
                CacheEntry<TValue> forRemove;
                _internalMap.TryRemove(sorted[i].Key, out forRemove);
            }
        }

        private class CacheEntry<T>
        {            
            internal DateTime _expiration;

            public T Value { get; set; }

            public CacheEntry(T item, DateTime expiration)
            {
                Value = item;                             
                _expiration = expiration;
			}

            public bool IsObsolet()
            {
                return TimeProvider.Current.Now  >= _expiration;
            }
		}
    }
}