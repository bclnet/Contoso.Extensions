using System;
using System.IO;
using Contoso.Extensions.Caching.Stream;
using Xunit;

namespace Contoso.Extensions.Caching.FileStream
{
    public class FileStreamCacheSetAndRemoveTests
    {
        [Fact]
        public void GetMissingKeyReturnsNull()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            long key = -1L; // non-existent-key

            var result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void SetAndGetReturnsObject()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            var value = new MemoryStream(new byte[1]);
            long key = 0L; // myKey

            key = cache.Set(key, value);

            var result = cache.Get(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void SetAlwaysOverwrites()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            var value1 = new MemoryStream(new byte[1] { 1 });
            long key = 0L; // myKey

            key = cache.Set(key, value1);
            var result = cache.Get(key);
            Assert.Equal(value1, result);

            var value2 = new MemoryStream(new byte[1] { 2 });
            key = cache.Set(key, value2);
            result = cache.Get(key);
            Assert.Equal(value2, result);
        }

        [Fact]
        public void RemoveRemoves()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            var value = new MemoryStream(new byte[1]);
            long key = 0L; // myKey

            key = cache.Set(key, value);
            var result = cache.Get(key);
            Assert.Equal(value, result);

            cache.Remove(key);
            result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void SetNullValueThrows()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            MemoryStream value = null;
            long key = 0L; // myKey

            Assert.Throws<ArgumentNullException>(() => cache.Set(key, value));
        }
    }
}
