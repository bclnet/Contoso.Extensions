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
            var key = "non-existent-key";

            var result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void SetAndGetReturnsObject()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            var value = new StreamWithHeader(new MemoryStream(new byte[1]));
            var key = "myKey";

            cache.Set(key, value);

            var result = cache.Get(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void SetAlwaysOverwrites()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            var value1 = new StreamWithHeader(new MemoryStream(new byte[1] { 1 }));
            var key = "myKey";

            cache.Set(key, value1);
            var result = cache.Get(key);
            Assert.Equal(value1, result);

            var value2 = new StreamWithHeader(new MemoryStream(new byte[1] { 2 }));
            cache.Set(key, value2);
            result = cache.Get(key);
            Assert.Equal(value2, result);
        }

        [Fact]
        public void RemoveRemoves()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);
            var value = new StreamWithHeader(new MemoryStream(new byte[1]));
            var key = "myKey";

            cache.Set(key, value);
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
            var key = "myKey";

            Assert.Throws<ArgumentNullException>(() => cache.Set(key, value));
        }
    }
}
