using System;
using System.Threading.Tasks;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.FileStream
{
    public class Database : IDisposable
    {
        public void Dispose()
        {
        }

        public void Close() => Dispose();

        public static Database GetDatabase(DatabaseOptions options, string instance)
        {
            return null;
        }

        public static Task<Database> GetDatabaseAsync(DatabaseOptions options, string instance)
        {
            return Task.FromResult<Database>(null);
        }

        public (IOStream, byte[][]) Get(long key, bool getData)
        {
            return (null, null);
        }

        public Task<(IOStream, byte[][])> GetAsync(long key, bool getData)
        {
            return Task.FromResult<(IOStream, byte[][])>((null, null));
        }

        public long Set(long? key, (IOStream, byte[][]) value)
        {
            return 0;
        }

        public Task<long> SetAsync(long? key, (IOStream, byte[][]) value)
        {
            return Task.FromResult<long>(0);
        }

        public void Delete(long key, TimeSpan? expr = null)
        {
        }

        public Task DeleteAsync(long key, TimeSpan? expr = null)
        {
            return Task.CompletedTask;
        }
    }
}