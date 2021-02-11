# Contoso.Extensions.Caching
extensions to the Microsoft.Extensions.Caching namespace

# Caching
The following are data services:

* `FileCacheDependency` - Configuration for FileCacheDependencies.
* `CacheItemBuilder` - The cache item builder delegate.
* `CacheItemBuilderAsync` - The cache item builder async delegate.

## FileCacheDependency
*Configuration for FileCacheDependencies.*

Method      | Type   | Description
---         | ---    | ---
Directory   | string | Gets or sets the directory.

### Example
```C#
FileCacheDependency.Directory = @"C:\";
```

## CacheItemBuilder
*The cache item builder delegate.*
```C#
delegate object CacheItemBuilder(object tag, object[] values)
```

## CacheItemBuilderAsync
*The cache item builder async delegate.*
```C#
delegate Task<object> CacheItemBuilderAsync(object tag, object[] values)
```



# Memory
The following are caching classes:

* `MemoryCacheExtensions` - Extensions to IMemoryCache.
* `MemoryCacheManager` - Manages the MemoryCache.
* `MemoryCacheResult` - Descriptive result object.
* `MemoryCacheRegistration` - A memory cache key and builder registration object.

## MemoryCacheExtensions
*Extensions to IMemoryCache.*

Method                    | Type   | Description
---                       | ---    | ---
GetEntriesCollection      | IDictionary | Gets the entries collection.
TryGetEntry               | bool   | Tries to get the entry.
Contains                  | bool   | Determines whether this instance contains the object.
SetCacheEntryChangeExpiration | MemoryCacheEntryOptions | Sets the cache entry change expiration.
SetFileWatchExpiration    | MemoryCacheEntryOptions | Sets the file watch expiration.
MakeCacheEntryChangeToken | IChangeToken | Makes the cache entry change token.
ForceAbsoluteExpiration   | MemoryCacheEntryOptions | Forces the absolute expiration.

### Example
```C#
static readonly IMemoryCache _cache = new MemoryCache(...);

var hasKey = _cache.Contains("Key");
```

## MemoryCacheManager
*Manages the MemoryCache.*

Method          | Type    | Description
---             | ---     | ---
Remove          | void    | Removes the specified key.
Contains        | bool    | Determines whether this instance contains the object.
Touch           | void    | Touches the specified names.
Get<T>          | T       | Gets the specified key.
GetResult       | MemoryCacheResult | Gets the specified key as MemoryCacheResult.
GetAsync<T>     | Task<T> | Gets the specified key.
GetResultAsync  | Task<MemoryCacheResult> | Gets the specified key as MemoryCacheResult.
Set             | void    | Sets the specified name with value.

### Example
```C#
static readonly IMemoryCache _cache = new MemoryCache(...);

_cache.Touch("MyKey");
```

## MemoryCacheResult
*Descriptive result object.*

Method          | Type    | Description
---             | ---     | ---
CacheResult     | MemoryCacheResult | A static empty cache result.
NoResult        | MemoryCacheResult | A static empty no-cache result.
Result          | object  | Gets the result.
Tag             | object  | Gets or sets the tag.
ETag            | string  | Gets or sets the e-tag.

### Example
```C#
static readonly IMemoryCache _cache = new MemoryCache(...);

var etag = _cache.GetResult("Key")?.ETag;
```


## MemoryCacheRegistration
*A memory cache key and builder registration object.*

Method          | Type    | Description
---             | ---     | ---
Name            | string  | Gets the name.
Builder         | CacheItemBuilder | Gets the builder.
BuilderAsync    | CacheItemBuilderAsync | Gets the builder asynchronous.
EntryOptions    | MemoryCacheEntryOptions | Gets the entry options.
CacheTags       | Func<object, object[], string[]> | Gets the cache tags.
PostEvictionCallback | PostEvictionDelegate | Gets or sets the post eviction callback.
GetName         | string  | Gets the name.

### Example
```C#
static readonly IMemoryCache _cache = new MemoryCache(...);
static readonly MemoryCacheRegistration MyKey = new MemoryCacheRegistration(nameof(MyKey), (tag, values) => Guid.New());

var key = _cache.Get<Guid>(MyKey);
```



# Distributed
The following are caching classes:

* `DistributedCacheExtensions` - Extensions to IDistributedCache.
* `DistributedCacheManager` - Manages the DistributedCache.
* `DistributedCacheResult` - Descriptive result object.
* `DistributedCacheRegistration` - A distributed cache key and builder registration object.

## DistributedCacheExtensions
*Extensions to IDistributedCache.*

Method                    | Type   | Description
---                       | ---    | ---
AddExpirationToken        | DistributedCacheEntryOptions2 | Expire the cache entry if the given IChangeToken expires.
SetCacheEntryChangeExpiration | DistributedCacheEntryOptions2 | Sets the cache entry change expiration.
SetFileWatchExpiration    | DistributedCacheEntryOptions2 | Sets the file watch expiration.
MakeCacheEntryChangeToken | IChangeToken | Makes the cache entry change token.
ForceAbsoluteExpiration   | DistributedCacheEntryOptions2 | Forces the absolute expiration.

### Example
```C#
static readonly IDistributedCache _cache = new DistributedCache(...);

var hasKey = _cache.Contains("Key");
```

## DistributedCacheManager
*Manages the DistributedCache.*

Method          | Type    | Description
---             | ---     | ---
Remove          | void    | Removes the specified key.
Contains        | bool    | Determines whether this instance contains the object.
Touch           | void    | Touches the specified names.
Get<T>          | T       | Gets the specified key.
GetResult       | MemoryCacheResult | Gets the specified key as DistributedCacheResult.
GetAsync<T>     | Task<T> | Gets the specified key.
GetResultAsync  | Task<MemoryCacheResult> | Gets the specified key as DistributedCacheResult.
Add             | void    | Adds the specified name.
Serialize       | Func<object, byte[]> | The serializer.
Deserialize     | Func<byte[], object> | The deserializer.

### Example
```C#
static readonly IDistributedCache _cache = new DistributedCache(...);

_cache.Touch("MyKey");
```

## DistributedCacheResult
*Descriptive result object.*

Method          | Type    | Description
---             | ---     | ---
CacheResult     | MemoryCacheResult | A static empty cache result.
NoResult        | MemoryCacheResult | A static empty no-cache result.
Result          | object  | Gets the result.
Tag             | object  | Gets or sets the tag.
ETag            | string  | Gets or sets the e-tag.

### Example
```C#
static readonly IDistributedCache _cache = new DistributedCache(...);

var result = _cache.GetResult("Key")?.ETag;
```

## DistributedCacheRegistration
*A distributed cache key and builder registration object.*

Method          | Type    | Description
---             | ---     | ---
Name            | string  | Gets the name.
Builder         | CacheItemBuilder | Gets the builder.
BuilderAsync    | CacheItemBuilderAsync | Gets the builder asynchronous.
EntryOptions    | DistributedCacheEntryOptions | Gets the entry options.
CacheTags       | Func<object, object[], string[]> | Gets the cache tags.
PostEvictionCallback | PostEvictionDelegate | Gets or sets the post eviction callback.
GetName         | string  | Gets the name.

### Example
```C#
static readonly IDistributedCache _cache = new DistributedCache(...);
static readonly DistributedCacheRegistration MyKey = new DistributedCacheRegistration(nameof(MyKey), (tag, values) => Guid.New());

var key = _cache.Get<Guid>(MyKey);
```