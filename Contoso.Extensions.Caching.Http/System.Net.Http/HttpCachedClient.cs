using Contoso.Extensions.Caching.Stream;
using System.Threading;
using System.Threading.Tasks;

// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
// https://www.keycdn.com/blog/http-cache-headers
// https://www.imperva.com/learn/performance/cache-control/
namespace System.Net.Http
{
    public class HttpCachedClient : HttpClient
    {
        readonly IStreamCache _cache;

        public HttpCachedClient(IStreamCache cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            _cache = cache;
        }

        public IStreamCache Cache => _cache;

        // SENDASYNC
        static bool CanCache(HttpRequestMessage request) => request.Method == HttpMethod.Get;
        
        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => CanCache(request)
            ? WrappedSendAsync(base.SendAsync(request, cancellationToken), request, cancellationToken)
            : base.SendAsync(request, cancellationToken);

        async Task<HttpResponseMessage> WrappedSendAsync(Task<HttpResponseMessage> @base, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = GetKey(request);
            var response = await GetCachedResponseAsync(key, request, cancellationToken);
            if (response != null)
            {
                return response;
            }
            response = await @base;
            await SetCachedResponseAsync(key, response, cancellationToken);
            var sameResponse = await GetCachedResponseAsync(key, request, cancellationToken);
            return response;
        }

        // SERDES
        static string GetKey(HttpRequestMessage request) => request.RequestUri.AbsoluteUri;

        async Task SetCachedResponseAsync(string key, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var value = HttpSerDes.SerializeResponseMessage(response);
            await _cache.SetAsync(key, value, cancellationToken);
        }

        async Task<HttpResponseMessage> GetCachedResponseAsync(string key, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var item = await _cache.GetAsync(key, cancellationToken);
            if (item == null)
                return null;
            if (!(item is StreamWithHeader value))
                throw new InvalidOperationException($"Cache must return a {nameof(StreamWithHeader)}");

            var response = HttpSerDes.DeserializeResponseMessage(value);
            response.RequestMessage = request;
            return response;
        }
    }
}