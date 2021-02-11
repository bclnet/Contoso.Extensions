using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// A memory cache key and builder registration object.
    /// </summary>
    public class MemoryCacheRegistration
    {
        internal readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, CacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, null, builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entryOptions">The entry options.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, CacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, entryOptions, builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, CacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, null, builder, null, cacheTags) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minuteTimeout">The minute timeout.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, int minuteTimeout, CacheItemBuilder builder, params string[] cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), builder, null, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minuteTimeout">The minute timeout.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, int minuteTimeout, CacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), builder, null, cacheTags) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entryOptions">The entry options.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, CacheItemBuilder builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, entryOptions, builder, null, cacheTags) { }
        //
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, CacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, null, null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entryOptions">The entry options.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, CacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, entryOptions, null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, CacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, null, null, builder, cacheTags) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minuteTimeout">The minute timeout.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, int minuteTimeout, CacheItemBuilderAsync builder, params string[] cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), null, builder, cacheTags != null && cacheTags.Length > 0 ? (a, b) => cacheTags : (Func<object, object[], string[]>)null) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minuteTimeout">The minute timeout.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, int minuteTimeout, CacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(minuteTimeout)), null, builder, cacheTags) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRegistration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="entryOptions">The entry options.</param>
        /// <param name="builder">The builder.</param>
        /// <param name="cacheTags">The cache tags.</param>
        public MemoryCacheRegistration(string name, MemoryCacheEntryOptions entryOptions, CacheItemBuilderAsync builder, Func<object, object[], string[]> cacheTags)
            : this(new StackTrace(), name, entryOptions, null, builder, cacheTags) { }
        MemoryCacheRegistration(StackTrace stackTrace, string name, MemoryCacheEntryOptions entryOptions, CacheItemBuilder builder, CacheItemBuilderAsync builderAsync, Func<object, object[], string[]> cacheTags)
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

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public CacheItemBuilder Builder { get; private set; }
        /// <summary>
        /// Gets the builder asynchronous.
        /// </summary>
        /// <value>
        /// The builder asynchronous.
        /// </value>
        public CacheItemBuilderAsync BuilderAsync { get; private set; }
        /// <summary>
        /// Gets the entry options.
        /// </summary>
        /// <value>
        /// The entry options.
        /// </value>
        public MemoryCacheEntryOptions EntryOptions { get; private set; }
        /// <summary>
        /// Gets the cache tags.
        /// </summary>
        /// <value>
        /// The cache tags.
        /// </value>
        public Func<object, object[], string[]> CacheTags { get; private set; }
        /// <summary>
        /// Gets or sets the post eviction callback.
        /// </summary>
        /// <value>
        /// The post eviction callback.
        /// </value>
        public PostEvictionDelegate PostEvictionCallback { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public string GetName(params object[] values)
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
