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

        public StreamWithHeader Get(string key, bool getData)
        {
            return null;
        }

        public Task<StreamWithHeader> GetAsync(string key, bool getData)
        {
            return Task.FromResult<StreamWithHeader>(null);
        }

        public void Set(string key, StreamWithHeader value)
        {
        }

        public Task SetAsync(string key, StreamWithHeader value)
        {
            return Task.CompletedTask;
        }

        public void Delete(string key, TimeSpan? expr = null)
        {
        }

        public Task DeleteAsync(string key, TimeSpan? expr = null)
        {
            return Task.CompletedTask;
        }
    }
}