using System.Threading.Tasks;
using Cache.Engine.Model;

namespace Cache.Engine
{
    public interface IStoreReader
    {
        Task<DocItem> ReadFromStore(DocItemKey oldKey);
    }
}