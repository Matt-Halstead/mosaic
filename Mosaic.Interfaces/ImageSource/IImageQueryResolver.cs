using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Interfaces.ImageSource
{
    // Contract for some object that can resolve queries on images.
    public interface IImageQueryResolver
    {
        Task<IImageQueryResult> Resolve(IImageQuery query, CancellationToken cancelToken);
    }
}
