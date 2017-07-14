namespace Cache.Engine.Configuration
{
    public interface IConfigurationProvider
    {
        CacheConfiguration LatestCacheConfiguration { get; }
    }
}