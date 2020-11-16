using Contoso.Extensions.Caching.Stream;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="HttpCachedClient"/> is shared.
        /// </summary>
        /// <value>
        ///   <c>true</c> if shared; otherwise, <c>false</c>.
        /// </value>
        public bool Shared { get; set; }

        // SENDASYNC
        static bool CanCache(HttpRequestMessage request) => request.Method == HttpMethod.Get;
        static string GetKey(HttpRequestMessage request) => $"{request.Method}:{request.RequestUri.AbsoluteUri}";

        /// <summary>
        /// Send an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => CanCache(request)
            ? WrappedSendAsync(() => base.SendAsync(request, cancellationToken), request, cancellationToken)
            : base.SendAsync(request, cancellationToken);

        async Task<HttpResponseMessage> WrappedSendAsync(Func<Task<HttpResponseMessage>> @base, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = GetKey(request);
            HttpResponseMessage response;
            var cachedResponse = await GetCachedResponseAsync(key, request, cancellationToken);
            if (cachedResponse == null)
                response = await @base();
            else
            {
                var cachedControl = new CacheControl(this, cachedResponse);
                if (cachedControl.Validation == null)
                    return cachedResponse;
                response = await cachedControl.Validation(@base, request);
                if (response.StatusCode == HttpStatusCode.NotModified)
                    return cachedResponse;
            }
            var cacheControl = new CacheControl(this, response);
            if (cacheControl.ShouldCache)
                await SetCachedResponseAsync(key, response, cacheControl.AbsoluteExpiration, cancellationToken);
            return response;
        }

        // https://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html
        // https://tools.ietf.org/html/rfc7234
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Caching
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
        class CacheControl
        {
            readonly DateTimeOffset Date;
            readonly CacheControlHeaderValue Value;
            public readonly bool ShouldCache;
            readonly TimeSpan? FreshnessLifetime;
            public readonly Func<Func<Task<HttpResponseMessage>>, HttpRequestMessage, Task<HttpResponseMessage>> Validation;
            public DateTimeOffset? AbsoluteExpiration => FreshnessLifetime != null ? (DateTimeOffset?)Date.Add(FreshnessLifetime.Value) : null;

            public CacheControl(HttpCachedClient parent, HttpResponseMessage response)
            {
                var headers = response.Headers;
                var contentHeaders = response.Content.Headers;
                Date = headers.Date ?? DateTimeOffset.UtcNow;
                Value = headers.CacheControl ?? CacheControlHeaderValue.Parse(headers.Pragma.ToString());
                ShouldCache = Value.Public || (Value.Private && parent.Shared) || !Value.NoStore;
                // 4.2.1.  Calculating Freshness Lifetime
                FreshnessLifetime = parent.Shared && Value.SharedMaxAge != null && Value.SharedMaxAge.Value.TotalSeconds != 0 ? Value.SharedMaxAge
                    : Value.MaxAge != null && Value.MaxAge.Value.TotalSeconds != 0 ? Value.MaxAge
                    : contentHeaders.Expires != null ? (TimeSpan?)contentHeaders.Expires.Value.Subtract(Date)
                    : contentHeaders.LastModified != null ? (TimeSpan?)TimeSpan.FromSeconds(contentHeaders.LastModified.Value.Subtract(Date).TotalSeconds / 10)
                    : null;
                Validation = !Value.NoCache && (Value.MaxAge == null || Value.MaxAge.Value.TotalSeconds != 0) ? (Func<Func<Task<HttpResponseMessage>>, HttpRequestMessage, Task<HttpResponseMessage>>)null
                    : headers.ETag != null ? (b, r) => IfNoneMatch(b, r, headers.ETag)
                    : contentHeaders.LastModified != null ? (b, r) => IfModifiedSince(b, r, contentHeaders.LastModified.Value)
                    : (Func<Func<Task<HttpResponseMessage>>, HttpRequestMessage, Task<HttpResponseMessage>>)null;
            }

            public async Task<HttpResponseMessage> IfNoneMatch(Func<Task<HttpResponseMessage>> @base, HttpRequestMessage request, EntityTagHeaderValue etag)
            {
                request.Headers.IfNoneMatch.Add(etag);
                var response = await @base();
                return response;
            }

            public async Task<HttpResponseMessage> IfModifiedSince(Func<Task<HttpResponseMessage>> @base, HttpRequestMessage request, DateTimeOffset lastModified)
            {
                request.Headers.IfModifiedSince = lastModified;
                var response = await @base();
                return response;
            }
        }

        // SERDES

        async Task SetCachedResponseAsync(string key, HttpResponseMessage response, DateTimeOffset? absoluteExpiration, CancellationToken cancellationToken)
        {
            var value = await HttpSerDes.SerializeResponseMessageAsync(response);
            await Cache.SetAsync(key, value, new StreamCacheEntryOptions { AbsoluteExpiration = absoluteExpiration }, cancellationToken);
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