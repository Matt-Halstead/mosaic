using System.Drawing;

namespace Mosaic.Interfaces.Processing
{
    public interface IImageFeatureExtractor
    {
        IImageFeatureSet ExtractFeatures(Image image);
    }
}
