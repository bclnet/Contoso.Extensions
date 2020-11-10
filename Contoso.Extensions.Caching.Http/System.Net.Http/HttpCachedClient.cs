using Contoso.Extensions.Caching.Stream;
using System.Threading;
using System.Threading.Tasks;

// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
// https://www.keycdn.com/blog/http-cache-headers
// https://www.imperva.com/learn/performance/cache-control/
namespace System.Net.Http
{
    /// <summary>
    /// HttpCachedClient
    /// </summary>
    /// <seealso cref="System.Net.Http.HttpClient" />
    public class HttpCachedClient : HttpClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCachedClient"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <exception cref="ArgumentNullException">cache</exception>
        public HttpCachedClient(IStreamCache cache) : base() => Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCachedClient"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="handler">The handler.</param>
        /// <exception cref="ArgumentNullException">cache</exception>
        public HttpCachedClient(IStreamCache cache, HttpMessageHandler handler) : base(handler) => Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCachedClient"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="disposeHandler">if set to <c>true</c> [dispose handler].</param>
        /// <exception cref="ArgumentNullException">cache</exception>
        public HttpCachedClient(IStreamCache cache, HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler) => Cache = cache ?? throw new ArgumentNullException(nameof(cache));

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        public IStreamCache Cache { get; }

        // SENDASYNC
        static bool CanCache(HttpRequestMessage request) => request.Method == HttpMethod.Get;

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => CanCache(request)
            ? WrappedSendAsync(base.SendAsync(request, cancellationToken), request, cancellationToken)
            : base.SendAsync(request, cancellationToken);

        async Task<HttpResponseMessage> WrappedSendAsync(Task<HttpResponseMessage> @base, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = GetKey(request);
            //var response = await GetCachedResponseAsync(key, request, cancellationToken);
            //if (response != null)
            //    return response;
            var response = await @base;
            await SetCachedResponseAsync(key, response, cancellationToken);
            response = await GetCachedResponseAsync(key, request, cancellationToken);
            return response;
        }

        // SERDES
        static string GetKey(HttpRequestMessage request) => request.RequestUri.AbsoluteUri;

        async Task SetCachedResponseAsync(string key, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var value = await HttpSerDes.SerializeResponseMessageAsync(response);
            await Cache.SetAsync(key, value, cancellationToken);
        }

        async Task<HttpResponseMessage> GetCachedResponseAsync(string key, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var item = await Cache.GetAsync(key, cancellationToken);
            if (item == null)
                return null;
            if (!(item is StreamWithHeader value))
                throw new InvalidOperationException($"Cache must return a {nameof(StreamWithHeader)}");

            var response = await HttpSerDes.DeserializeResponseMessageAsync(value);
            response.RequestMessage = request;
            return response;
        }
    }
}