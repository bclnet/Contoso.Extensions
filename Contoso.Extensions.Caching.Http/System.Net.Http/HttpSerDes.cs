using Contoso.Extensions.Caching.Stream;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Net.Http
{
    /// <summary>
    /// HttpSerDes
    /// </summary>
    public static class HttpSerDes
    {
        static readonly Type ResponseContentType = typeof(HttpContent).Assembly.GetType("System.Net.Http.HttpConnectionResponseContent");
        static readonly MethodInfo SetStreamMethod = ResponseContentType?.GetMethod("SetStream");

        /// <summary>
        /// Serializes the response message asynchronous.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unable to find HttpConnectionResponseContent</exception>
        /// <exception cref="ArgumentNullException">response</exception>
        public static async Task<StreamWithHeader> SerializeResponseMessageAsync(HttpResponseMessage response)
        {
            if (ResponseContentType == null)
                throw new InvalidOperationException("Unable to find HttpConnectionResponseContent");
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var headers = response.Headers.ToDictionary(a => a.Key, a => a.Value);
            using (var s = new MemoryStream())
            {
                var f = new BinaryWriter(s);
                f.Write((int)response.StatusCode);
                f.Write(response.Version.ToString());
                f.Write(response.ReasonPhrase != null); if (response.ReasonPhrase != null) f.Write(response.ReasonPhrase);
                f.Write(headers);
                var header = Compress(s.ToArray());

                var @base = await response.Content.ReadAsStreamAsync();
                //var @base = new MemoryStream();
                //await response.Content.CopyToAsync(@base);

                @base.Position = 0;
                return new StreamWithHeader(@base, header);
            }
        }

        /// <summary>
        /// Deserializes the response message asynchronous.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unable to find HttpConnectionResponseContent</exception>
        /// <exception cref="ArgumentNullException">source or Header</exception>
        public static Task<HttpResponseMessage> DeserializeResponseMessageAsync(StreamWithHeader source)
        {
            if (ResponseContentType == null)
                throw new InvalidOperationException("Unable to find HttpConnectionResponseContent");
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source.Header == null)
                throw new ArgumentNullException(nameof(source.Header));

            using (var s = new MemoryStream(Decompress(source.Header)))
            {
                var f = new BinaryReader(s);
                var response = new HttpResponseMessage((HttpStatusCode)f.ReadInt32())
                {
                    Version = new Version(f.ReadString()),
                    ReasonPhrase = f.ReadBoolean() ? f.ReadString() : null,
                };
                var headers = f.ReadDictionary();
                foreach (var header in headers)
                    response.Headers.Add(header.Key, header.Value);

                if (source.Base.CanSeek)
                    source.Base.Position = 0;
                var content = new StreamContent(source.Base);
                //var content = (HttpContent)Activator.CreateInstance(ResponseContentType);
                //SetStreamMethod.Invoke(content, new[] { source.Base });

                response.Content = content;
                return Task.FromResult(response);
            }
        }

        static void Write(this BinaryWriter s, Dictionary<string, IEnumerable<string>> value)
        {
            s.Write(value.Count);
            foreach (var kv in value)
            {
                s.Write(kv.Key);
                s.Write(kv.Value != null);
                if (kv.Value != null)
                {
                    s.Write(kv.Value.Count());
                    foreach (var v in kv.Value)
                    {
                        s.Write(v != null); if (v != null) s.Write(v);
                    }
                }
            }
        }

        static Dictionary<string, IEnumerable<string>> ReadDictionary(this BinaryReader s)
        {
            var r = new Dictionary<string, IEnumerable<string>>();
            var count = s.ReadInt32();
            for (var kv = 0; kv < count; kv++)
            {
                var key = s.ReadString();
                if (s.ReadBoolean())
                {
                    var list = new List<string>();
                    var count2 = s.ReadInt32();
                    for (var v = 0; v < count2; v++)
                        list.Add(s.ReadBoolean() ? s.ReadString() : null);
                    r.Add(key, list);
                }
            }
            return r;
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