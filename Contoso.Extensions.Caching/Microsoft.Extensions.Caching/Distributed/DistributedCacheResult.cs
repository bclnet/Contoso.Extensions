using System;

namespace Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    /// Descriptive result object.
    /// </summary>
    public class DistributedCacheResult
    {
        /// <summary>
        /// A static empty cache result.
        /// </summary>
        public static readonly DistributedCacheResult CacheResult = new DistributedCacheResult();
        /// <summary>
        /// A static empty no-cache result.
        /// </summary>
        public static readonly DistributedCacheResult NoResult = new DistributedCacheResult();
        DistributedCacheResult() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheResult"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public DistributedCacheResult(object result) => Result = result;
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; private set; }
        internal DistributedCacheRegistration Key { get; set; }
        internal DistributedCacheEntryOptions EntryOptions { get; set; }
        internal WeakReference WeakTag { get; set; }
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public object Tag { get; set; }
        /// <summary>
        /// Gets or sets the e-tag.
        /// </summary>
        /// <value>
        /// The e tag.
        /// </value>
        public string ETag { get; set; }
    }
}
