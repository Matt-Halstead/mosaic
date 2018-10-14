using Mosaic.Interfaces.ImageSource;

namespace Mosaic.ImageSource.Processing
{
    // Contract for a series of descriptive properties extracted from an image.
    public interface IImageFeatureSet
    {
        IImage Image { get; }
    }
}
