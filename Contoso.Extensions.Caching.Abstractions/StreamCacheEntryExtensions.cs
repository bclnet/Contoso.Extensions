using System;

namespace Contoso.Extensions.Caching.Stream
{
    public static class StreamCacheEntryExtensions
    {
        /// <summary>
        /// Sets an absolute expiration time, relative to now.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="relative"></param>
        public static StreamCacheEntryOptions SetAbsoluteExpiration(
            this StreamCacheEntryOptions options,
            TimeSpan relative)
        {
            options.AbsoluteExpirationRelativeToNow = relative;
            return options;
        }

        /// <summary>
        /// Sets an absolute expiration date for the cache entry.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="absolute"></param>
        public static StreamCacheEntryOptions SetAbsoluteExpiration(
            this StreamCacheEntryOptions options,
            DateTimeOffset absolute)
        {
            options.AbsoluteExpiration = absolute;
            return options;
        }

        /// <summary>
        /// Sets how long the cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        /// <param name="options"></param>
        /// <param name="offset"></param>
        public static StreamCacheEntryOptions SetSlidingExpiration(
            this StreamCacheEntryOptions options,
            TimeSpan offset)
        {
            options.SlidingExpiration = offset;
            return options;
        }
    }
}