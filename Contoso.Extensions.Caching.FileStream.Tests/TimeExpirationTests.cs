using System;
using System.IO;
using System.Threading;
using Contoso.Extensions.Caching.Stream;
using Xunit;

namespace Contoso.Extensions.Caching.FileStream
{
    public class TimeExpirationTests
    {
        [Fact]
        public void AbsoluteExpirationInThePastThrows()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            var expected = DateTimeOffset.Now - TimeSpan.FromMinutes(1);
            Assert.Throws<ArgumentOutOfRangeException>(nameof(StreamCacheEntryOptions.AbsoluteExpiration), () =>
            {
                cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(expected));
            }); //"The absolute expiration value must be in the future.", expected);
        }

        [Fact]
        public void AbsoluteExpirationExpires()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(1)));

            var result = cache.Get(key);
            Assert.Equal(value, result);

            for (var i = 0; i < 4 && result != null; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                result = cache.Get(key);
            }

            Assert.Null(result);
        }

        [Fact]
        public void AbsoluteSubSecondExpirationExpiresImmidately()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(0.25)));

            var result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void NegativeRelativeExpirationThrows()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            Assert.Throws<ArgumentOutOfRangeException>(nameof(StreamCacheEntryOptions.AbsoluteExpirationRelativeToNow), () =>
            {
                cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(-1)));
            }); // "The relative expiration value must be positive.", TimeSpan.FromMinutes(-1));
        }

        [Fact]
        public void ZeroRelativeExpirationThrows()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            Assert.Throws<ArgumentOutOfRangeException>(nameof(StreamCacheEntryOptions.AbsoluteExpirationRelativeToNow), () =>
            {
                cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.Zero));
            }); //"The relative expiration value must be positive.", TimeSpan.Zero);
        }

        [Fact]
        public void RelativeExpirationExpires()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(1)));

            var result = cache.Get(key);
            Assert.Equal(value, result);

            for (var i = 0; i < 4 && result != null; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                result = cache.Get(key);
            }
            Assert.Null(result);
        }

        [Fact]
        public void RelativeSubSecondExpirationExpiresImmediately()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(0.25)));

            var result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void NegativeSlidingExpirationThrows()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            Assert.Throws<ArgumentOutOfRangeException>(nameof(StreamCacheEntryOptions.SlidingExpiration), () =>
            {
                cache.Set(key, value, new StreamCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(-1)));
            }); // "The sliding expiration value must be positive.", TimeSpan.FromMinutes(-1));
        }

        [Fact]
        public void ZeroSlidingExpirationThrows()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            Assert.Throws<ArgumentOutOfRangeException>(nameof(StreamCacheEntryOptions.SlidingExpiration), () =>
            {
                cache.Set(key, value, new StreamCacheEntryOptions().SetSlidingExpiration(TimeSpan.Zero));
            }); // "The sliding expiration value must be positive.", TimeSpan.Zero);
        }

        [Fact]
        public void SlidingExpirationExpiresIfNotAccessed()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(1)));

            var result = cache.Get(key);
            Assert.Equal(value, result);

            Thread.Sleep(TimeSpan.FromSeconds(3));

            result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void SlidingSubSecondExpirationExpiresImmediately()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(0.25)));

            var result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void SlidingExpirationRenewedByAccess()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(1)));

            var result = cache.Get(key);
            Assert.Equal(value, result);

            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));

                result = cache.Get(key);
                Assert.Equal(value, result);
            }

            Thread.Sleep(TimeSpan.FromSeconds(3));
            result = cache.Get(key);
            Assert.Null(result);
        }

        [Fact]
        public void SlidingExpirationRenewedByAccessUntilAbsoluteExpiration()
        {
            var cache = FileStreamTestConfig.CreateCacheInstance(GetType().Name);

            var key = 0L; // myKey
            var value = new MemoryStream(new byte[1]);

            cache.Set(key, value, new StreamCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(1))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3)));

            var result = cache.Get(key);
            Assert.Equal(value, result);

            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));

                result = cache.Get(key);
                Assert.Equal(value, result);
            }

            Thread.Sleep(TimeSpan.FromSeconds(.6));

            result = cache.Get(key);
            Assert.Null(result);
        }
    }
}
