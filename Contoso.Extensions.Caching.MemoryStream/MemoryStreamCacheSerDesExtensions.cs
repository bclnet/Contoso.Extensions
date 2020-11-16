using Contoso.Extensions.Caching.Stream;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using IOMemoryStream = System.IO.MemoryStream;
using IOStream = System.IO.Stream;

namespace Contoso.Extensions.Caching.MemoryStream
{
    /// <summary>
    /// Extension methods for setting up MemoryStream SerDes.
    /// </summary>
    public static class MemoryStreamCacheSerDesExtensions
    {
        static readonly PropertyInfo EntriesCollectionProp = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The custom type serializers
        /// </summary>
        public static readonly Dictionary<Type, (Action<BinaryWriter, FileStream, object> w, Func<BinaryReader, FileStream, object> r)> CustomTypeSerializers
            = new Dictionary<Type, (Action<BinaryWriter, FileStream, object> w, Func<BinaryReader, FileStream, object> r)>{
                {typeof(StreamWithHeader), (SerializeStreamWithHeader, DeserializeStreamWithHeader) }
            };

        static void SerializeStreamWithHeader(BinaryWriter f, FileStream s, object value)
        {
            var streamWithHeader = (StreamWithHeader)value;
            var @base = streamWithHeader.Base;
            var header = streamWithHeader.Header;
            f.Write(header != null); if (header != null) { f.Write(header.Length); f.Write(header); }
            f.Write(@base.Length);
            @base.CopyTo(s);
            if (@base.CanSeek)
                @base.Position = 0;
        }

        static object DeserializeStreamWithHeader(BinaryReader f, FileStream s)
        {
            var header = f.ReadBoolean() ? f.ReadBytes(f.ReadInt32()) : null;
            var baseLength = f.ReadInt64();
            var @base = s.CopyTill(new IOMemoryStream(), baseLength);
            return new StreamWithHeader(@base, header);
        }

        /// <summary>
        /// Copies the till.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Saves to file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="ArgumentOutOfRangeException">source</exception>
        public static void SaveToFile(this MemoryStreamCache source, string path)
        {
            var memCache = source._memCache;
            if (!(EntriesCollectionProp.GetValue(memCache) is ICollection collection))
                throw new ArgumentOutOfRangeException(nameof(source));

            using (var s = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var f = new BinaryWriter(s);
                f.Write(collection.Count);
                foreach (var item in collection)
                {
                    var p = s.Position;
                    try
                    {
                        var value = (ICacheEntry)item.GetType().GetProperty("Value").GetValue(item);
                        var valueType = value.Value?.GetType() ?? typeof(object);
                        var valueSer = CustomTypeSerializers.TryGetValue(valueType, out var z) ? z.w : (a, b, c) => throw new NotSupportedException(); // a.Serialize(b, c);

                        // serialize
                        f.Write(true);
                        f.Write(valueType.AssemblyQualifiedName);
                        f.WriteKey(value.Key);
                        f.Write(value.Value != null); if (value.Value != null) valueSer(f, s, value.Value);
                        f.Write(value.AbsoluteExpiration != null); if (value.AbsoluteExpiration != null) f.Write(value.AbsoluteExpiration.Value);
                        f.Write(value.AbsoluteExpirationRelativeToNow != null); if (value.AbsoluteExpirationRelativeToNow != null) f.Write(value.AbsoluteExpirationRelativeToNow.Value);
                        f.Write(value.SlidingExpiration != null); if (value.SlidingExpiration != null) f.Write(value.SlidingExpiration.Value);
                        f.Write((int)value.Priority);
                        f.Write(value.Size != null); if (value.Size != null) f.Write(value.Size.Value);
                    }
                    catch
                    {
                        s.Position = p;
                        f.Write(false);
                    }
                }
            }
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="path">The path.</param>
        public static void LoadFromFile(this MemoryStreamCache source, string path)
        {
            if (!File.Exists(path))
                return;
            var memCache = source._memCache;

            using (var s = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryReader(s);
                var count = f.ReadInt32();
                for (var idx = 0; idx < count; idx++)
                    if (f.ReadBoolean())
                    {
                        var valueType = Type.GetType(f.ReadString());
                        var valueDes = CustomTypeSerializers.TryGetValue(valueType, out var z) ? z.r : (a, b) => throw new NotSupportedException(); // a.Deserialize(b);
                        // deserialize
                        memCache.Set(
                            f.ReadKey(),
                            f.ReadBoolean() ? valueDes(f, s) : null,
                            new MemoryCacheEntryOptions
                            {
                                AbsoluteExpiration = f.ReadBoolean() ? (DateTimeOffset?)f.ReadDateTimeOffset() : null,
                                AbsoluteExpirationRelativeToNow = f.ReadBoolean() ? (TimeSpan?)f.ReadTimeSpan() : null,
                                SlidingExpiration = f.ReadBoolean() ? (TimeSpan?)f.ReadTimeSpan() : null,
                                Priority = (CacheItemPriority)f.ReadInt32(),
                                Size = f.ReadBoolean() ? (long?)f.ReadInt64() : null,
                            });
                    }
            }
        }

        static void WriteKey(this BinaryWriter s, object value) { if (value is string key) s.Write(key); else throw new NotSupportedException(); }
        static object ReadKey(this BinaryReader s) => s.ReadString();
        static void Write(this BinaryWriter s, TimeSpan value) => s.Write(value.Ticks);
        static TimeSpan ReadTimeSpan(this BinaryReader s) => new TimeSpan(s.ReadInt64());
        static void Write(this BinaryWriter s, DateTimeOffset value) { s.Write(value.Ticks); s.Write(value.Offset.Ticks); }
        static DateTimeOffset ReadDateTimeOffset(this BinaryReader s) => new DateTimeOffset(s.ReadInt64(), new TimeSpan(s.ReadInt64()));
    }
}