using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public delegate object DistributedCacheItemBuilder(object tag, object[] values);
    public delegate Task<object> DistributedCacheItemBuilderAsync(object tag, object[] values);
    public class DistributedCacheRegistration
    {
        internal readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public DistributedCacheRegistration(string name, DistributedCacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, null, builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public DistributedCacheRegistration(string name, DistributedCacheEntryOptions entryOptions, DistributedCacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, entryOptions, builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public DistributedCacheRegistration(string name, DistributedCacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, null, builder, null, cacheTags) { }
        public DistributedCacheRegistration(string name, int minuteTimeout, DistributedCacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public DistributedCacheRegistration(string name, int minuteTimeout, DistributedCacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), builder, null, cacheTags) { }
        public DistributedCacheRegistration(string name, DistributedCacheEntryOptions entryOptions, DistributedCacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, entryOptions, builder, null, cacheTags) { }
        //
        public DistributedCacheRegistration(string name, DistributedCacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, null, null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public DistributedCacheRegistration(string name, DistributedCacheEntryOptions entryOptions, DistributedCacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, entryOptions, null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public DistributedCacheRegistration(string name, DistributedCacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, null, null, builder, cacheTags) { }
        public DistributedCacheRegistration(string name, int minuteTimeout, DistributedCacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public DistributedCacheRegistration(string name, int minuteTimeout, DistributedCacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), null, builder, cacheTags) { }
        public DistributedCacheRegistration(string name, DistributedCacheEntryOptions entryOptions, DistributedCacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, entryOptions, null, builder, cacheTags) { }
        DistributedCacheRegistration(StackTrace stackTrace, string name, DistributedCacheEntryOptions entryOptions, DistributedCacheItemBuilder builder, DistributedCacheItemBuilderAsync builderAsync, Func<object, object[], string[]> cacheTags)
        {
            if (builder == null && builderAsync == null)
                throw new ArgumentNullException(nameof(builder));
            var parentName = stackTrace.GetFrame(1).GetMethod().DeclaringType.FullName;
            Name = $"{parentName}:{name}";
            EntryOptions = entryOptions ?? new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(90));
            Builder = builder;
            BuilderAsync = builderAsync;
            CacheTags = cacheTags;
        }

        public string Name { get; internal set; }
        public DistributedCacheItemBuilder Builder { get; private set; }
        public DistributedCacheItemBuilderAsync BuilderAsync { get; private set; }
        public DistributedCacheEntryOptions EntryOptions { get; private set; }
        public Func<object, object[], string[]> CacheTags { get; private set; }

        internal string GetNamespace(object[] values)
        {
            if (values == null || values.Length == 0)
                return Name;
            var b = new StringBuilder(Name);
            b.Append(":");
            foreach (var v in values)
            {
                if (v != null)
                    b.Append(v.ToString());
                b.Append("\\");
            }
            return b.ToString();
        }
    }
}
