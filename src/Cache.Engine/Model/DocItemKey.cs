namespace Cache.Engine.Model
{
    public class DocItemKey
    {
        private readonly string partition;
        private readonly string key;

        public DocItemKey(string partition, string key)
        {
            this.partition = partition;
            this.key = key;
        }

        public string Partition => partition;

        public string Key => key;

        public override string ToString()
        {
            return $"{nameof(partition)}: {partition}, {nameof(key)}: {key}";
        }

        protected bool Equals(DocItemKey other)
        {
            return string.Equals(partition, other.partition) && string.Equals(key, other.key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DocItemKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((partition != null ? partition.GetHashCode() : 0) * 397) ^ (key != null ? key.GetHashCode() : 0);
            }
        }
    }
}