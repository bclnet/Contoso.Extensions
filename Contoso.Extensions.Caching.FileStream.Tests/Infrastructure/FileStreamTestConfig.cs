using Contoso.Extensions.Caching.Stream;

namespace Contoso.Extensions.Caching.FileStream
{
    public static class FileStreamTestConfig
    {
        public static IStreamCache CreateCacheInstance(string instanceName) =>
            new FileStreamCache(new FileStreamCacheOptions
            {
                Configuration = "CONFIG",
                InstanceName = instanceName,
            });

        public static void GetOrStartServer()
        {
        }
    }
}
