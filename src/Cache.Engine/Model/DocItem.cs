namespace Cache.Engine.Model
{
    public class DocItem
    {
        public DocItem(DocItemKey key)
        {
            Key = key;
        }

        public DocItemKey Key { get; private set; }
    }
}