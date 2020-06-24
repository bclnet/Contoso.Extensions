using Contoso.Extensions.Caching.Stream;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpSerDes
    {
        static readonly Type ResponseContentType = typeof(HttpContent).Assembly.GetType("System.Net.Http.HttpConnectionResponseContent");
        static readonly MethodInfo SetStreamMethod = ResponseContentType?.GetMethod("SetStream");

        public static async Task<StreamWithHeader> SerializeResponseMessageAsync(HttpResponseMessage response)
        {
            if (ResponseContentType == null)
                throw new InvalidOperationException("Unable to find HttpConnectionResponseContent");
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var headers = response.Headers.ToDictionary(a => a.Key, a => a.Value);
            using (var s = new MemoryStream())
            {
                var f = new BinaryFormatter();
                f.Serialize(s, (int)response.StatusCode);
                f.Serialize(s, response.Version.ToString());
                f.Serialize(s, response.ReasonPhrase != null);
                if (response.ReasonPhrase != null) f.Serialize(s, response.ReasonPhrase);
                f.Serialize(s, headers);
                var header = Compress(s.ToArray());

                var @base = await response.Content.ReadAsStreamAsync();
                //var @base = new MemoryStream();
                //await response.Content.CopyToAsync(@base);

                @base.Position = 0;
                return new StreamWithHeader(@base, header);
            }
        }

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
                var f = new BinaryFormatter();
                var response = new HttpResponseMessage((HttpStatusCode)(int)f.Deserialize(s))
                {
                    Version = new Version((string)f.Deserialize(s)),
                    ReasonPhrase = (bool)f.Deserialize(s) ? (string)f.Deserialize(s) : null,
                };
                var headers = (Dictionary<string, IEnumerable<string>>)f.Deserialize(s);
                foreach (var header in headers)
                    response.Headers.Add(header.Key, header.Value);

                var content = new StreamContent(source.Base);
                //var content = (HttpContent)Activator.CreateInstance(ResponseContentType);
                //SetStreamMethod.Invoke(content, new[] { source.Base });

                response.Content = content;
                return Task.FromResult(response);
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