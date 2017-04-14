using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace PasswordGeneratorTelegramProxy.Models
{
    public class BidirectionalConcurrentDictionary<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> _internalDictionary;
    }
}