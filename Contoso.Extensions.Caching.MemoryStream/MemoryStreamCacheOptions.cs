using Microsoft.Extensions.Caching.Memory;

namespace Contoso.Extensions.Caching.MemoryStream
{
    /// <summary>
    /// Configuration options for <see cref="MemoryStreamCache"/>.
    /// </summary>
    public class MemoryStreamCacheOptions : MemoryCacheOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStreamCacheOptions"/> class.
        /// </summary>
        public MemoryStreamCacheOptions() : base()
        {
            // Default size limit of 200 MB
            SizeLimit = 200 * 1024 * 1024;
        }
    }
}