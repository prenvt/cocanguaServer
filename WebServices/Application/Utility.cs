using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

public class Utility
{
    private static MemoryCache _cache;

    public static void Init()
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100 * 1000,
        });
    }

    #region Cac Ham Lam Viec Voi Cache
    /*public static object AddCacheNotRemovable(string key, object value, DateTime absoluteTime, TimeSpan slidingTime, CacheItemRemovedCallback callback)
    {
        return HttpRuntime.Cache.Add(key, value, null, absoluteTime, slidingTime, CacheItemPriority.NotRemovable, callback);
    }*/

    public static void RemoveCache(String key)
    {
        //HttpRuntime.Cache.Remove(key);
        _cache.Remove(key);
    }

    /*public static void InsertCache(String key, object value, DateTime cacheTime)
    {
        //HttpRuntime.Cache.Insert(key, value, null, cacheTime, Cache.NoSlidingExpiration);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60 * 20));
        _cache.Set(key, value, cacheEntryOptions);
    }

    public static void InsertCache(String key, object value)
    {
        int cacheTime = 3600 * 4;
        HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddSeconds(cacheTime), Cache.NoSlidingExpiration);
    }*/

    public static object GetCacheObject(String key)
    {
        //return HttpRuntime.Cache.Get(key);
        object cacheObj = null;
        _cache.TryGetValue(key, out cacheObj);
        return cacheObj;
    }

    public static T GetCache<T>(string key)
    {
        //return (T)HttpRuntime.Cache.Get(key);
        return (T)GetCacheObject(key);
    }

    public static void InsertCache(string key, object value, int cacheSeconds = 600, bool removeIfExist = false)
    {
        if (removeIfExist)
        {
            RemoveCache(key);
        }
        //HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddSeconds(cacheSeconds), Cache.NoSlidingExpiration);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromSeconds(cacheSeconds));
        _cache.Set(key, value, cacheEntryOptions);
    }
    #endregion
}