using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using System;

namespace Contoso.Extensions.Caching.FileSystem
{
    /// <summary>
    /// Configuration options for <see cref="FileSystemCache"/>.
    /// </summary>
    public class FileSystemCacheOptions : IOptions<FileSystemCacheOptions>
    {
        /// <summary>
        /// The configuration used to connect to the File System.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to the File System.
        /// This is preferred over Configuration.
        /// </summary>
        public DatabaseOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// The File System instance name.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// An abstraction to represent the clock of a machine in order to enable unit testing.
        /// </summary>
        public ISystemClock SystemClock { get; set; }

        /// <summary>
        /// The periodic interval to scan and delete expired items in the cache. Default is 30 minutes.
        /// </summary>
        public TimeSpan? ExpiredItemsDeletionInterval { get; set; }

        /// <summary>
        /// The default sliding expiration set for a cache entry if neither Absolute or SlidingExpiration has been set explicitly.
        /// By default, its 20 minutes.
        /// </summary>
        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);

        FileSystemCacheOptions IOptions<FileSystemCacheOptions>.Value
        {
            get { return this; }
        }
    }
}