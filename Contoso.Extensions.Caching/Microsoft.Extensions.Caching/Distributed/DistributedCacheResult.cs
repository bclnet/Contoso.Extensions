using System;

namespace Microsoft.Extensions.Caching.Distributed
{
    public class DistributedCacheResult
    {
        public DistributedCacheResult(object result) => Result = result;
        public object Result { get; private set; }
        internal DistributedCacheRegistration Key { get; set; }
        internal DistributedCacheEntryOptions EntryOptions { get; set; }
        internal WeakReference WeakTag { get; set; }
        public object Tag { get; set; }
        public string ETag { get; set; }
    }
}
