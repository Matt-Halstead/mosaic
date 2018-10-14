using Mosaic.Interfaces.ImageSource;
using System.Threading.Tasks;

namespace Mosaic.ImageSource.Processing
{
    public interface IMosaicBuilder
    {
        Task<IMosaic> BuildMosaic(IImageSource imageSource, IImageFeatureExtractor featureExtractor);
    }
}
