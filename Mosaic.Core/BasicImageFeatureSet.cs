using Mosaic.Interfaces.Processing;
using Mosaic.Interfaces.ImageSource;
using System.Drawing;

namespace Mosaic.Core
{
    internal class BasicImageFeatureSet : IImageFeatureSet
    {
        public BasicImageFeatureSet(Image image)
        {
            Image = image;
        }

        public Image Image { get; private set; }

        internal void Extract()
        {
            // todo
        }
    }
}
