using System;

namespace Cache.Engine.Model
{
    public class EmptyDocItem
    {
        static readonly DocItem empty = new DocItem(new DocItemKey(String.Empty, String.Empty));
        public static DocItem Create(DocItemKey oldKey)
        {
            return empty;
        }
    }
}