using Microsoft.Extensions.Caching.Memory;

namespace Contoso.Extensions.Caching.MemoryStream
{
    /// <summary>
    /// Configuration options for <see cref="MemoryStreamCache"/>.
    /// </summary>
    public class MemoryStreamCacheOptions : MemoryCacheOptions
    {
        public MemoryStreamCacheOptions()
            : base()
        {
            // Default size limit of 200 MB
            SizeLimit = 200 * 1024 * 1024;
        }
    }
}