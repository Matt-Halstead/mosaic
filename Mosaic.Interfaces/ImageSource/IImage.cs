using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Mosaic.Interfaces.ImageSource
{
    // Contract for some object that contains image data with related meta data.
    public interface IImage
    {
        Image RawImage { get; }
        string Id { get; }
        string Title { get; }
        List<string> Tags { get; }
        Uri Link { get; }

        Task<IImage> LoadImage();
    }
}
