using System;

namespace Contoso.Extensions.Caching.Stream
{
    /// <summary>
    /// Provides the cache options for an entry in <see cref="IStreamCache"/>.
    /// </summary>
    public class StreamCacheEntryOptions
    {
        DateTimeOffset? _absoluteExpiration;
        TimeSpan? _absoluteExpirationRelativeToNow;
        TimeSpan? _slidingExpiration;

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration
        {
            get => _absoluteExpiration;
            set => _absoluteExpiration = value;
        }

        /// <summary>
        /// Gets or sets an absolute expiration time, relative to now.
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get => _absoluteExpirationRelativeToNow;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(AbsoluteExpirationRelativeToNow), value, "The relative expiration value must be positive.");
                _absoluteExpirationRelativeToNow = value;
            }
        }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public TimeSpan? SlidingExpiration
        {
            get => _slidingExpiration;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(SlidingExpiration), value, "The sliding expiration value must be positive.");
                _slidingExpiration = value;
            }
        }
    }
}