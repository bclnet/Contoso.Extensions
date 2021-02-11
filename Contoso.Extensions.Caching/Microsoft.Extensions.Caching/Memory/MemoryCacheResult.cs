using System;

namespace Microsoft.Extensions.Caching.Memory
{
    /// <summary>
    /// Descriptive result object.
    /// </summary>
    public class MemoryCacheResult
    {
        /// <summary>
        /// A static empty cache result.
        /// </summary>
        public static readonly MemoryCacheResult CacheResult = new MemoryCacheResult();
        /// <summary>
        /// A static empty no-cache result.
        /// </summary>
        public static readonly MemoryCacheResult NoResult = new MemoryCacheResult();
        MemoryCacheResult() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheResult"/> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public MemoryCacheResult(object result) => Result = result;
        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public object Result { get; private set; }
        internal MemoryCacheRegistration Key { get; set; }
        internal MemoryCacheEntryOptions EntryOptions { get; set; }
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
