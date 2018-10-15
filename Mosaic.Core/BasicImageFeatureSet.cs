using Mosaic.Interfaces.Processing;
using System;
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
        public byte MeanRed { get; set; }
        public byte MeanGreen { get; set; }
        public byte MeanBlue { get; set; }

        public bool IsMatch(IImageFeatureSet other)
        {
            if (other is BasicImageFeatureSet otherBasic)
            {
                return FuzzyEquals(MeanRed, otherBasic.MeanRed, 5);
            }

            return false;
        }

        private bool FuzzyEquals(byte a, byte b, byte tol)
        {
            return Math.Abs(a - b) < tol;
        }
    }
}
