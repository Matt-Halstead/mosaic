using Mosaic.Interfaces.ImageSource;

namespace Mosaic.ImageSource.Processing
{
    public interface IImageFeatureExtractor
    {
        IImageFeatureSet ExtractFeatures(IImage image);
    }
}
