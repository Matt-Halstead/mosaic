using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Interfaces.ImageSource
{
    public interface IImageSource
    {
        Task<IEnumerable<IImage>> GetImages(IImageQueryResolver queryResolver, IImageQuery query, CancellationToken cancelToken);
    }
}
