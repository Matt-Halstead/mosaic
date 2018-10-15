using System.Collections.Generic;

namespace Mosaic.Interfaces.ImageSource
{
    public interface IImageQueryResult
    {
        List<IImage> Images { get; }
    }
}
