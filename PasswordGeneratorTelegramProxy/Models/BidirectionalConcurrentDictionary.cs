using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class BidirectionalConcurrentDictionary<T1, T2>
    {
        private ConcurrentDictionary<T1, T2> _forward;
        private ConcurrentDictionary<T2, T1> _reverse;

        public BidirectionalConcurrentDictionary()
        {
            _forward = new ConcurrentDictionary<T1, T2>();
            _reverse = new ConcurrentDictionary<T2, T1>();
        }

        public bool TryAdd(T1 item1, T2 item2)
        {
            bool fSuccess = _forward.TryAdd(item1, item2);
            bool rSuccess = _reverse.TryAdd(item2, item1);

            if (fSuccess && !rSuccess)
            {
                T1 temp;
                _reverse.TryRemove(item2, out temp);
            }

            if (rSuccess && !fSuccess)
            {
                T2 temp;
                _forward.TryRemove(item1, out temp);
            }

            return fSuccess && rSuccess;
        }

        public T1 this[T2 key]
        {
            get { return _reverse[key]; }
        }

        public T2 this[T1 key]
        {
            get { return _forward[key]; }
        }
    }
}