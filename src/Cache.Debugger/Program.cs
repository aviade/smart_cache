using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Cache.Engine;
using Cache.Engine.Configuration;
using Cache.Engine.Model;

namespace Cache.Debugger
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryCacheStore cacheStore = new MemoryCacheStore(
                MemoryCache.Default, 
                new ConfigurationProvider(), new StoreReader(), 
                new ConsoleLogger());

            string input;
            while ((input = Console.ReadLine()) != "q")
            {
                if (string.IsNullOrEmpty(input))
                {
                    break;

                }
                if (input.StartsWith("w:"))
                {
                    string value = input.Substring(2);
                    Write(cacheStore, value);
                }
                else if (input.StartsWith("r:"))
                {
                    string value = input.Substring(2);
                    Read(cacheStore, value);
                }

            }

            cacheStore.Dispose();
        }

        private static void Read(MemoryCacheStore cacheStore, string value)
        {
            var key = GetKey(value);
            var docItem = cacheStore[key];
        }

        private static void Write(MemoryCacheStore cacheStore, string value)
        {
            var key = GetKey(value);
            cacheStore.Write(new List<DocItem>()
            {
                new DocItem(key),
            });
        }

        private static DocItemKey GetKey(string value)
        {
            return new DocItemKey(String.Empty, value);
        }
    }

    internal class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        public void TraceInfo(string s)
        {
            Trace.WriteLine(s);
        }

        public void TraceError(string s)
        {
            Trace.TraceError(s);
        }
    }

    internal class StoreReader : IStoreReader
    {
        public async Task<DocItem> ReadFromStore(DocItemKey oldKey)
        {
            // Return fake result
            return await Task.FromResult(new DocItem(new DocItemKey(string.Empty, Guid.NewGuid().ToString())));
        }
    }


    internal class ConfigurationProvider : IConfigurationProvider
    {
        public ConfigurationProvider()
        {
            LatestCacheConfiguration = new CacheConfiguration()
            {
                LowerHitsThreshold = 3,
                WaitIntervalDefault = TimeSpan.FromSeconds(30),
            };
        }

        public CacheConfiguration LatestCacheConfiguration { get; }
    }
}
