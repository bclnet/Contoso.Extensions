using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching
{
    /// <summary>
    /// The cache item builder delegate.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    public delegate object CacheItemBuilder(object tag, object[] values);

    /// <summary>
    /// The cache item builder async delegate.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    public delegate Task<object> CacheItemBuilderAsync(object tag, object[] values);
}
