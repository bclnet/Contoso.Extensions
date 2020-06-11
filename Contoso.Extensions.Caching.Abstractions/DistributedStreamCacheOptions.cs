//using Microsoft.Extensions.Options;

//namespace Contoso.Extensions.Caching.Stream
//{
//    /// <summary>
//    /// Configuration options for <see cref="DistributedStreamCache"/>.
//    /// </summary>
//    public class DistributedStreamCacheOptions : IOptions<DistributedStreamCacheOptions>
//    {
//        /// <summary>
//        /// The configuration used to connect to the FileStream.
//        /// </summary>
//        public string Configuration { get; set; }

//        /// <summary>
//        /// The configuration used to connect to the FileStream.
//        /// This is preferred over Configuration.
//        /// </summary>
//        public DatabaseOptions ConfigurationOptions { get; set; }

//        /// <summary>
//        /// The FileStream instance name.
//        /// </summary>
//        public string InstanceName { get; set; }

//        DistributedStreamCacheOptions IOptions<DistributedStreamCacheOptions>.Value
//        {
//            get { return this; }
//        }
//    }
//}