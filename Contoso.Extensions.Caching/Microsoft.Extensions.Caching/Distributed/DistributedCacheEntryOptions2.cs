using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace Microsoft.Extensions.Caching.Distributed
{
    /// <summary>
    /// Class DistributedCacheEntryOptions2.
    /// Implements the <see cref="Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions" />
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions" />
    public class DistributedCacheEntryOptions2 : DistributedCacheEntryOptions
    {
        public DistributedCacheEntryOptions ToEntryOptions() => this;

        /// <summary>
        /// Gets the <see cref="IChangeToken"/> instances which cause the cache entry to expire.
        /// </summary>
        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

        //DateTimeOffset _absoluteExpiration;
        //TimeSpan _floatingAbsoluteExpiration;

        //public ServiceCacheEntryOptions() { }
        //public ServiceCacheEntryOptions(int floatingAbsoluteMinuteTimeout)
        //{
        //    if (floatingAbsoluteMinuteTimeout < -1)
        //        throw new ArgumentOutOfRangeException(nameof(floatingAbsoluteMinuteTimeout));
        //    if (floatingAbsoluteMinuteTimeout >= 0) _floatingAbsoluteExpiration = new TimeSpan(0, floatingAbsoluteMinuteTimeout, 0);
        //    else _absoluteExpiration = DateTimeOffset.MaxValue;
        //}

        //public new DateTimeOffset AbsoluteExpiration
        //{
        //    get
        //    {
        //        if (SlidingExpiration != TimeSpan.Zero) return DateTimeOffset.MinValue;
        //        if (_absoluteExpiration == DateTime.MinValue && _floatingAbsoluteExpiration == TimeSpan.Zero) return DateTimeOffset.Now.Add(new TimeSpan(1, 0, 0));
        //        return _floatingAbsoluteExpiration != TimeSpan.Zero ? DateTimeOffset.Now.Add(_floatingAbsoluteExpiration) : _absoluteExpiration;
        //    }
        //    set
        //    {
        //        if (_floatingAbsoluteExpiration != TimeSpan.Zero)
        //            throw new InvalidOperationException("FloatingExpiration already set");
        //        _absoluteExpiration = value;
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the DateTime instance that represent the absolute expiration of the item being added to cache.
        ///// </summary>
        ///// <value>The absolute expiration.</value>
        //public TimeSpan FloatingAbsoluteExpiration
        //{
        //    get
        //    {
        //        if (SlidingExpiration != TimeSpan.Zero)
        //            return TimeSpan.Zero;
        //        return _floatingAbsoluteExpiration;
        //    }
        //    set
        //    {
        //        if (_absoluteExpiration != DateTime.MinValue)
        //            throw new InvalidOperationException("AbsoluteExpiration already set");
        //        if (value < TimeSpan.Zero)
        //            throw new ArgumentOutOfRangeException("value");
        //        _floatingAbsoluteExpiration = value;
        //    }
        //}

        //public DistributedCacheEntryOptions ToItemPolicy()
        //{
        //    if (_absoluteExpiration == DateTimeOffset.MaxValue)
        //        return this;
        //    var r = new DistributedCacheEntryOptions
        //    {
        //        AbsoluteExpiration = AbsoluteExpiration,
        //        Priority = Priority,
        //        RemovedCallback = base.RemovedCallback,
        //        SlidingExpiration = SlidingExpiration,
        //        UpdateCallback = UpdateCallback,
        //    };
        //    foreach (var x in ChangeMonitors)
        //        r.ChangeMonitors.Add(x);
        //    return r;
        //}

        //Action<object, CacheEntryRemovedArguments> _removedCallback;
        //public new Action<object, CacheEntryRemovedArguments> RemovedCallback
        //{
        //    get { return _removedCallback; }
        //    set
        //    {
        //        _removedCallback = value;
        //        if (value != null) base.RemovedCallback = args => value(args.CacheItem.Value is ServiceCacheResult ? ((ServiceCacheResult)args.CacheItem.Value).WeakTag.Target : null, args);
        //        else base.RemovedCallback = null;
        //    }
        //}
    }
}
