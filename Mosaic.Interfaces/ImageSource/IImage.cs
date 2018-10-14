using System.Drawing;

namespace Mosaic.Interfaces.ImageSource
{
    // Contract for some object that contains image data with related meta data.
    public interface IImage
    {
        Image RawImage { get; }
    }
}
