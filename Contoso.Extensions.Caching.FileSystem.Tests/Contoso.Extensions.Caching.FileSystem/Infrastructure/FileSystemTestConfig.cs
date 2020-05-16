using Microsoft.Extensions.Caching.Distributed;

namespace Contoso.Extensions.Caching.FileSystem
{
    public static class FileSystemTestConfig
    {
        public static IDistributedCache CreateCacheInstance(string instanceName) =>
            new FileSystemCache(new FileSystemCacheOptions
            {
                Configuration = "CONFIG",
                InstanceName = instanceName,
            });

        public static void GetOrStartServer()
        {
        }
    }
}
