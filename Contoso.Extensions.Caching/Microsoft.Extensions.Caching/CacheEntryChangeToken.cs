using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;

namespace Microsoft.Extensions.Caching
{
    public class CacheEntryChangeToken : IChangeToken
    {
        public void PostEvictionCallback(object key, object value, EvictionReason reason, object state) => HasChanged = true;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => null;

        public bool ActiveChangeCallbacks => false;

        public bool HasChanged { get; set; }
    }
}
