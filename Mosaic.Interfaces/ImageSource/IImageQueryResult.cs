using System.Collections.Generic;

namespace Mosaic.Interfaces.ImageSource
{
    public interface IImageQueryResult
    {
        IEnumerable<IImage> Images { get; }
    }
}
