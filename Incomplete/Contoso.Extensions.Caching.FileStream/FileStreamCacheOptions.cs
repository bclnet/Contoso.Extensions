using Microsoft.Extensions.Options;

namespace Contoso.Extensions.Caching.FileStream
{
    /// <summary>
    /// Configuration options for <see cref="FileStreamCache"/>.
    /// </summary>
    public class FileStreamCacheOptions : IOptions<FileStreamCacheOptions>
    {
        /// <summary>
        /// The configuration used to connect to the FileStream.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to the FileStream.
        /// This is preferred over Configuration.
        /// </summary>
        public DatabaseOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// The FileStream instance name.
        /// </summary>
        public string InstanceName { get; set; }

        FileStreamCacheOptions IOptions<FileStreamCacheOptions>.Value
        {
            get { return this; }
        }
    }
}