﻿using Contoso.Extensions.Caching.Stream;
using System.Threading;
using System.Threading.Tasks;

// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
// https://www.keycdn.com/blog/http-cache-headers
// https://www.imperva.com/learn/performance/cache-control/
namespace System.Net.Http
{
    public class HttpCachedClient : HttpClient
    {
        public HttpCachedClient(IStreamCache cache) : base() => Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        public HttpCachedClient(IStreamCache cache, HttpMessageHandler handler) : base(handler) => Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        public HttpCachedClient(IStreamCache cache, HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler) => Cache = cache ?? throw new ArgumentNullException(nameof(cache));

        public IStreamCache Cache { get; }

        // SENDASYNC
        static bool CanCache(HttpRequestMessage request) => request.Method == HttpMethod.Get;

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => CanCache(request)
            ? WrappedSendAsync(base.SendAsync(request, cancellationToken), request, cancellationToken)
            : base.SendAsync(request, cancellationToken);

        async Task<HttpResponseMessage> WrappedSendAsync(Task<HttpResponseMessage> @base, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = GetKey(request);
            //var response = await GetCachedResponseAsync(key, request, cancellationToken);
            //if (response != null)
            //{
            //    return response;
            //}
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