using System;

namespace Cache.Engine.Configuration
{
    public class CacheConfiguration
    {
        public TimeSpan WaitIntervalDefault { get; set; }
        public int LowerHitsThreshold { get; set; }
    }
}