using Contoso.Extensions.Caching.Stream;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Net.Http
{
    public static class HttpSerDes
    {
        public static StreamWithHeader SerializeResponseMessage(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var headers = response.Headers.ToDictionary(a => a.Key, a => a.Value);
            using (var s = new MemoryStream())
            {
                var f = new BinaryFormatter();
                f.Serialize(s, response.Version.ToString());
                f.Serialize(s, (int)response.StatusCode);
                f.Serialize(s, headers);
                var compressed = Compress(s.ToArray());
                return new StreamWithHeader(new MemoryStream(), compressed);
            }
        }

        public static HttpResponseMessage DeserializeResponseMessage(StreamWithHeader source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source.Header == null)
                throw new ArgumentNullException(nameof(source.Header));

            using (var s = new MemoryStream(Decompress(source.Header)))
            {
                var f = new BinaryFormatter();
                var response = new HttpResponseMessage
                {
                    Version = new Version((string)f.Deserialize(s)),
                    StatusCode = (HttpStatusCode)(int)f.Deserialize(s),
                };
                var headers = (Dictionary<string, IEnumerable<string>>)f.Deserialize(s);
                foreach (var header in headers)
                    response.Headers.Add(header.Key, header.Value);
                return response;
            }
        }

        static byte[] Compress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
                    zip.Write(input, 0, input.Length);
                return outputStream.ToArray();
            }
        }

        static byte[] Decompress(byte[] input)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream(input))
                using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    zip.CopyTo(outputStream);
                return outputStream.ToArray();
            }
        }
    }
}