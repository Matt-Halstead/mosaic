using System.Drawing;

namespace Mosaic.Interfaces.Processing
{
    // Contract for a series of descriptive properties extracted from an image.
    public interface IImageFeatureSet
    {
        Image Image { get; }

        bool IsMatch(IImageFeatureSet other);
    }
}
