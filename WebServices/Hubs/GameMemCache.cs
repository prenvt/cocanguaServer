using Microsoft.Extensions.Caching.Memory;

public class GameMemCache
{
    public MemoryCache Cache { get; set; }
    public GameMemCache()
    {
        Cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 100 * 1000,
        });
    }
}