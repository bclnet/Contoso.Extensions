using System;

namespace Microsoft.Extensions.Caching.Memory
{
    public class MemoryCacheResult
    {
        public static readonly MemoryCacheResult CacheResult = new MemoryCacheResult();
        public static readonly MemoryCacheResult NoResult = new MemoryCacheResult();
        MemoryCacheResult() { }
        public MemoryCacheResult(object result) => Result = result;
        public object Result { get; private set; }
        internal MemoryCacheRegistration Key { get; set; }
        internal MemoryCacheEntryOptions EntryOptions { get; set; }
        internal WeakReference WeakTag { get; set; }
        public object Tag { get; set; }
        public string ETag { get; set; }
    }
}
