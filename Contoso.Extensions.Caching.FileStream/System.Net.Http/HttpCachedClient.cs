using Contoso.Extensions.Caching.Stream;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public class HttpCachedClient : HttpClient
    {
        readonly IStreamCache _cache;

        public HttpCachedClient(IStreamCache cache)
        {
            if (_cache == null)
                throw new ArgumentNullException(nameof(_cache));
            _cache = cache;
        }

        static bool CanCache(HttpRequestMessage request) => request.Method == HttpMethod.Get;

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!CanCache(request))
                return base.SendAsync(request, cancellationToken);
            return base.SendAsync(request, cancellationToken);
        }
    }
}