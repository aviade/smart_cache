using System.Threading;

namespace Cache.Engine.Model
{
    public class DocItemCache
    {
        public DocItem Item { get; set; }

        private int hits;
        public DocItemCache(DocItem item)
        {
            Item = item;
        }

        public int Hits => hits;

        public int IncHits()
        {
            return Interlocked.Increment(ref hits);
        }

    }
}