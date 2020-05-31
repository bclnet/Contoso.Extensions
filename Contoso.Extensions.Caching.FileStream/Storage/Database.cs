using Contoso.Extensions.Caching.Stream;
using System;
using System.Threading.Tasks;

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

        public MetadataStream<byte[][]> Get(long key, bool getData)
        {
            return null;
        }

        public Task<MetadataStream<byte[][]>> GetAsync(long key, bool getData)
        {
            return Task.FromResult<MetadataStream<byte[][]>>(null);
        }

        public long Set(long? key, MetadataStream<byte[][]> value)
        {
            return 0;
        }

        public Task<long> SetAsync(long? key, MetadataStream<byte[][]> value)
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