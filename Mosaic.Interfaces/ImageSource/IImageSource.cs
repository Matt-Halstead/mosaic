using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Interfaces.ImageSource
{
    public interface IImageSource
    {
        Task<IImage> GetNextImage(CancellationToken cancelToken);
    }
}
