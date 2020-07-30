using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using IOStream = System.IO.Stream;
using IOMemoryStream = System.IO.MemoryStream;

namespace Contoso.Extensions.Caching.MemoryStream
{
    /// <summary>
    /// Extension methods for setting up MemoryStream SerDes.
    /// </summary>
    public static class MemoryStreamCacheSerDesExtensions
    {
        static readonly PropertyInfo EntriesCollectionProp = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);

        public static readonly Dictionary<Type, (Action<BinaryFormatter, FileStream, object>, Func<BinaryFormatter, FileStream, object>)> CustomTypeSerializers
            = new Dictionary<Type, (Action<BinaryFormatter, FileStream, object>, Func<BinaryFormatter, FileStream, object>)>{
                {typeof(StreamWithHeader), (SerializeStreamWithHeader, DeserializeStreamWithHeader) }
            };

        static void SerializeStreamWithHeader(BinaryFormatter f, FileStream s, object value)
        {
            var streamWithHeader = (StreamWithHeader)value;
            var @base = streamWithHeader.Base;
            var header = streamWithHeader.Header;
            f.Serialize(s, header != null);
            if (header != null) f.Serialize(s, header);
            f.Serialize(s, @base.Length);
            @base.CopyTo(s);
        }

        static object DeserializeStreamWithHeader(BinaryFormatter f, FileStream s)
        {
            var header = (bool)f.Deserialize(s) ? (byte[])f.Deserialize(s) : null;
            var baseLength = (long)f.Deserialize(s);
            var @base = s.CopyTill(new IOMemoryStream(), baseLength);
            return new StreamWithHeader(@base, header);
        }

        public static IOStream CopyTill(this IOStream input, IOStream output, long bytes)
        {
            int read;
            var buffer = new byte[32768];
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, (int)bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
            return output;
        }

        public static void SaveToFile(this MemoryStreamCache source, string path)
        {
            return;
            var memCache = source._memCache;
            if (!(EntriesCollectionProp.GetValue(memCache) is ICollection collection))
                throw new ArgumentOutOfRangeException(nameof(source));

            using (var s = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var f = new BinaryFormatter();
                f.Serialize(s, collection.Count);
                foreach (var item in collection)
                {
                    var p = s.Position;
                    try
                    {
                        var value = (ICacheEntry)item.GetType().GetProperty("Value").GetValue(item);
                        var valueType = value.Value?.GetType() ?? typeof(object);
                        var valueSer = CustomTypeSerializers.TryGetValue(valueType, out var z) ? z.Item1 : (a, b, c) => a.Serialize(b, c);

                        // serialize
                        f.Serialize(s, true);
                        f.Serialize(s, valueType.AssemblyQualifiedName);
                        f.Serialize(s, value.Key);
                        f.Serialize(s, value.Value != null);
                        if (value.Value != null) valueSer(f, s, value.Value);
                        f.Serialize(s, value.AbsoluteExpiration.HasValue);
                        if (value.AbsoluteExpiration.HasValue) f.Serialize(s, value.AbsoluteExpiration.Value);
                        f.Serialize(s, value.AbsoluteExpirationRelativeToNow.HasValue);
                        if (value.AbsoluteExpiration.HasValue) f.Serialize(s, value.AbsoluteExpirationRelativeToNow.Value);
                        f.Serialize(s, value.SlidingExpiration.HasValue);
                        if (value.SlidingExpiration.HasValue) f.Serialize(s, value.SlidingExpiration.Value);
                        f.Serialize(s, (int)value.Priority);
                        f.Serialize(s, value.Size.HasValue);
                        if (value.Size.HasValue) f.Serialize(s, value.Size.Value);
                    }
                    catch
                    {
                        s.Position = p;
                        f.Serialize(s, false);
                    }
                }
            }
        }

        public static void LoadFromFile(this MemoryStreamCache source, string path)
        {
            return;
            if (!File.Exists(path))
                return;
            var memCache = source._memCache;

            using (var s = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryFormatter();
                var count = (int)f.Deserialize(s);
                for (var idx = 0; idx < count; idx++)
                    if ((bool)f.Deserialize(s))
                    {
                        var valueType = Type.GetType((string)f.Deserialize(s));
                        var valueDes = CustomTypeSerializers.TryGetValue(valueType, out var z) ? z.Item2 : (a, b) => a.Deserialize(b);
                        // deserialize
                        memCache.Set(
                            f.Deserialize(s),
                            (bool)f.Deserialize(s) ? valueDes(f, s) : null,
                            new MemoryCacheEntryOptions
                            {
                                AbsoluteExpiration = (bool)f.Deserialize(s) ? (DateTimeOffset?)f.Deserialize(s) : null,
                                AbsoluteExpirationRelativeToNow = (bool)f.Deserialize(s) ? (TimeSpan?)f.Deserialize(s) : null,
                                SlidingExpiration = (bool)f.Deserialize(s) ? (TimeSpan?)f.Deserialize(s) : null,
                                Priority = (CacheItemPriority)(int)f.Deserialize(s),
                                Size = (bool)f.Deserialize(s) ? (long?)f.Deserialize(s) : null,
                            });
                    }
            }
        }
    }
}