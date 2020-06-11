using Contoso.Extensions.Caching.MemoryStream;
using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up MemoryStream caching related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class MemoryStreamCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the MemoryStream caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMemoryStreamCache(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<IStreamCache, MemoryStreamCache>());

            return services;
        }

        /// <summary>
        /// Adds the MemoryStream caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{FileStreamCacheOptions}"/> to configure the provided <see cref="MemoryStreamCacheOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMemoryStreamCache(this IServiceCollection services, Action<MemoryStreamCacheOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddMemoryStreamCache();
            services.Configure(setupAction);

            return services;
        }

        static readonly PropertyInfo EntriesCollectionProp = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void SaveToFile(this MemoryStreamCache source, string path)
        {
            var memCache = source._memCache;
            if (!(EntriesCollectionProp.GetValue(memCache) is ICollection collection))
                throw new ArgumentOutOfRangeException(nameof(source));

            using (var s = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var f = new BinaryFormatter();
                f.Serialize(s, collection.Count);
                foreach (var item in collection)
                {
                    var p = s.Position;
                    try
                    {
                        var value = (ICacheEntry)item.GetType().GetProperty("Value").GetValue(item);
                        f.Serialize(s, true);
                        f.Serialize(s, value.Key);
                        f.Serialize(s, value.Value);
                        f.Serialize(s, value.AbsoluteExpiration.HasValue);
                        if (value.AbsoluteExpiration.HasValue) f.Serialize(s, value.AbsoluteExpiration.Value);
                        f.Serialize(s, value.AbsoluteExpirationRelativeToNow.HasValue);
                        if (value.AbsoluteExpiration.HasValue) f.Serialize(s, value.AbsoluteExpirationRelativeToNow.Value);
                        f.Serialize(s, value.SlidingExpiration.HasValue);
                        if (value.SlidingExpiration.HasValue) f.Serialize(s, value.SlidingExpiration.Value);
                        f.Serialize(s, (int)value.Priority);
                        f.Serialize(s, value.Size.HasValue);
                        if (value.Size.HasValue) f.Serialize(s, value.Size.Value);
                    }
                    catch
                    {
                        s.Position = p;
                        f.Serialize(s, false);
                    }
                }
            }
        }

        public static void LoadFromFile(this MemoryStreamCache source, string path)
        {
            if (!File.Exists(path))
                return;
            var memCache = source._memCache;

            using (var s = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryFormatter();
                var count = (int)f.Deserialize(s);
                for (var idx = 0; idx < count; idx++)
                    if ((bool)f.Deserialize(s))
                        memCache.Set(
                            f.Deserialize(s),
                            f.Deserialize(s),
                            new MemoryCacheEntryOptions
                            {
                                AbsoluteExpiration = (bool)f.Deserialize(s) ? (DateTimeOffset?)f.Deserialize(s) : null,
                                AbsoluteExpirationRelativeToNow = (bool)f.Deserialize(s) ? (TimeSpan?)f.Deserialize(s) : null,
                                SlidingExpiration = (bool)f.Deserialize(s) ? (TimeSpan?)f.Deserialize(s) : null,
                                Priority = (CacheItemPriority)(int)f.Deserialize(s),
                                Size = (bool)f.Deserialize(s) ? (long?)f.Deserialize(s) : null,
                            });
            }
        }
    }
}