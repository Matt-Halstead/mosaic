using Mosaic.Interfaces.Processing;
using System.Drawing;

namespace Mosaic.Core
{
    public class BasicImageFeatureExtractor : IImageFeatureExtractor
    {
        public IImageFeatureSet ExtractFeatures(Image image)
        {
            var features = new BasicImageFeatureSet(image);
            features.Extract();
            return features;
        }
    }
}
