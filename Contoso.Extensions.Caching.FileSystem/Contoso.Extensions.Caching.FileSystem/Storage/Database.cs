using System;
using System.Threading.Tasks;

namespace Contoso.Extensions.Caching.FileSystem
{
    public class Database : IDisposable
    {
        public void Dispose()
        {
        }

        public void Close() => Dispose();

        public static Database GetDatabase(DatabaseOptions options)
        {
            return null;
        }

        public static Task<Database> GetDatabaseAsync(DatabaseOptions options)
        {
            return Task.FromResult<Database>(null);
        }

        public void KeyDelete(string v)
        {
        }

        public Task KeyDeleteAsync(string v)
        {
            return Task.CompletedTask;
        }

        public void KeyExpire(string v, TimeSpan? expr)
        {
        }

        public Task KeyExpireAsync(string v, TimeSpan? expr)
        {
            return Task.CompletedTask;
        }
    }
}