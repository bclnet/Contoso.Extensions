using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Memory
{
    public delegate object MemoryCacheItemBuilder(object tag, object[] values);
    public delegate Task<object> MemoryCacheItemBuilderAsync(object tag, object[] values);
    public class MemoryCacheRegistration
    {
        internal readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public MemoryCacheRegistration(string name, MemoryCacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, null, builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, MemoryCacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, entryOptions, builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public MemoryCacheRegistration(string name, MemoryCacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, null, builder, null, cacheTags) { }
        public MemoryCacheRegistration(string name, int minuteTimeout, MemoryCacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public MemoryCacheRegistration(string name, int minuteTimeout, MemoryCacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), builder, null, cacheTags) { }
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, MemoryCacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, entryOptions, builder, null, cacheTags) { }
        //
        public MemoryCacheRegistration(string name, MemoryCacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, null, null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, MemoryCacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, entryOptions, null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public MemoryCacheRegistration(string name, MemoryCacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, null, null, builder, cacheTags) { }
        public MemoryCacheRegistration(string name, int minuteTimeout, MemoryCacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        public MemoryCacheRegistration(string name, int minuteTimeout, MemoryCacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), null, builder, cacheTags) { }
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, MemoryCacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, entryOptions, null, builder, cacheTags) { }
        MemoryCacheRegistration(StackTrace stackTrace, string name, MemoryCacheEntryOptions entryOptions, MemoryCacheItemBuilder builder, MemoryCacheItemBuilderAsync builderAsync, Func<object, object[], string[]> cacheTags)
        {
            if (builder == null && builderAsync == null)
                throw new ArgumentNullException(nameof(builder));
            var parentName = stackTrace.GetFrame(1).GetMethod().DeclaringType.FullName;
            Name = $"{parentName}:{name}";
            EntryOptions = entryOptions ?? new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(90));
            Builder = builder;
            BuilderAsync = builderAsync;
            CacheTags = cacheTags;
        }

        public string Name { get; internal set; }
        public MemoryCacheItemBuilder Builder { get; private set; }
        public MemoryCacheItemBuilderAsync BuilderAsync { get; private set; }
        public MemoryCacheEntryOptions EntryOptions { get; private set; }
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
