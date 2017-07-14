using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Cache.Engine.Configuration;
using Cache.Engine.Model;

namespace Cache.Engine
{
    public interface IMemoryCacheStore : IDisposable
    {
        DocItem this[DocItemKey key] { get; }
        void Write(IList<DocItem> allItems);
        void UpdateStoreReader(IStoreReader reader);
    }

    public class MemoryCacheStore : IMemoryCacheStore
    {
        private readonly IConfigurationProvider configurationProvider;
        private IStoreReader storeReader;

        private MemoryCache memCache;
        private readonly ILogger logger;

        public MemoryCacheStore(
            MemoryCache memoryCache,
            IConfigurationProvider provider,
            IStoreReader storeReader,
            ILogger logger)
        {
            memCache = memoryCache;
            this.configurationProvider = provider;
            this.storeReader = storeReader;
            this.logger = logger;
        }

        private CacheConfiguration LatestCacheConfiguration
        {
            get
            {
                var configuration = configurationProvider.LatestCacheConfiguration;
                return configuration;
            }
        }

        public DocItem this[DocItemKey key]
        {
            get
            {
                var docItemCache = (DocItemCache)memCache?[key.ToString()];
                if (docItemCache == null)
                {
                    return null;
                }
                int totalHits = docItemCache.IncHits();
                logger.TraceInfo($"Temp cache: Inc Hits to {totalHits} for {key}");
                return docItemCache.Item;
            }
        }

        public void UpdateStoreReader(IStoreReader reader)
        {
            storeReader = reader;
        }

        public void Write(IList<DocItem> allItems)
        {
            foreach (DocItem item in allItems)
            {
                var policy = GetPolicy();
                Set(item.Key, new DocItemCache(item), policy);
                this.logger.TraceInfo($"Temp cache: Adding Key: {item.Key}");
            }
        }

        private void UpdateCallback(CacheEntryUpdateArguments arguments)
        {
            var expired = arguments.RemovedReason == CacheEntryRemovedReason.Expired;
            if (!expired)
            {
                logger.TraceError(
                    $"Temp cache: Can't refresh cache since remove reason is {arguments.RemovedReason}. Key:{arguments.Key}");
                return;
            }
            var oldItem = (DocItemCache)memCache?[arguments.Key];
            if (oldItem == null)
            {
                // If memcache was disposed don't error out
                if (memCache != null)
                {
                    logger.TraceError(
                        $"Temp cache: Can't refresh cache since item is not in the cache. Key:{arguments.Key}");
                }

                return;
            }
            var hits = oldItem.Hits;
            var lowerHitsThreshold = LatestCacheConfiguration.LowerHitsThreshold;

            if (hits < lowerHitsThreshold)
            {
                logger.TraceInfo(
                    $"Temp cache: Will not refresh cache item with {hits} hits. LowerHitsThreshold: {lowerHitsThreshold}. Key: {arguments.Key}");
                return;
            }
            logger.TraceInfo(
                $"Temp cache: Refreshing cache. Hits: {hits} higher than {lowerHitsThreshold}. Key: {arguments.Key}");

            var reader = storeReader;

            try
            {
                var oldKey = oldItem.Item.Key;
                DocItem freshDocItem = reader.ReadFromStore(oldKey).GetAwaiter().GetResult();
                freshDocItem = freshDocItem ?? EmptyDocItem.Create(oldKey);
                var docItemCache = new DocItemCache(freshDocItem);
                var cacheItem = new CacheItem(oldKey.ToString(), docItemCache);
                arguments.UpdatedCacheItem = cacheItem;
                arguments.UpdatedCacheItemPolicy = GetPolicy();

                logger.TraceInfo(
                    $"Temp cache: Cache refreshed! Key: {arguments.Key}");
            }
            catch (Exception e)
            {
                logger.TraceError(
                    $"Temp cache: Failed to refresh cache. Key:{arguments.Key}. Error: {e}");
            }
        }

        public void Set(DocItemKey key, DocItemCache docItemCache, CacheItemPolicy policy)
        {
            var cacheItem = new CacheItem(key.ToString(), docItemCache);
            memCache?.Set(cacheItem, policy);
        }

        private CacheItemPolicy GetPolicy()
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add(LatestCacheConfiguration.WaitIntervalDefault),
                UpdateCallback = UpdateCallback
            };
            return policy;
        }

        public void Dispose()
        {
            memCache.Dispose();
            memCache = null;
        }
    }

    public enum CacheStoreResult
    {
        NotChanged
    }
}