using Mosaic.Interfaces.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Mosaic.Core
{
    public class BasicImageFeatureExtractor : IImageFeatureExtractor
    {
        public IImageFeatureSet ExtractFeatures(Image image)
        {
            var features = new BasicImageFeatureSet(image);

            // for now just use a greyscale histo on one channel
            var histo = new ColorHistogram(image);
            features.MeanRed = histo.RedHistogram.MeanOfNPercentile(0.5f);
            //features.MeanGreen = histo.GreenHistogram.Mean();
            //features.MeanBlue = histo.BlueHistogram.Mean();
            return features;
        }
    }

    internal class ColorHistogram
    {
        //private readonly Dictionary<Color, int> _histo = new Dictionary<Color, int>();

        public GreyScaleHistogram RedHistogram { get; private set; }
        //public GreyScaleHistogram GreenHistogram { get; private set; }
        //public GreyScaleHistogram BlueHistogram { get; private set; }

        public ColorHistogram(Image image)
        {
            RedHistogram = new GreyScaleHistogram(image, GreyScaleHistogram.Channel.R);
            //GreenHistogram = new GreyScaleHistogram(image, GreyScaleHistogram.Channel.G);
            //BlueHistogram = new GreyScaleHistogram(image, GreyScaleHistogram.Channel.B);

            //Bitmap bm = (Bitmap)image;

            //for (int x = 0; x < bm.Width; x++)
            //{
            //    for (int y = 0; y < bm.Height; y++)
            //    {
            //        Color c = bm.GetPixel(x, y);

            //        if (_histo.TryGetValue(c, out int count))
            //        {
            //            _histo[c] = count + 1;
            //        }
            //        else
            //        {
            //            _histo.Add(c, 1);
            //        }
            //    }
            //}
        }
    }

    internal class GreyScaleHistogram
    {
        public enum Channel { R, G, B };

        private readonly Dictionary<byte, int> _histo = new Dictionary<byte, int>();

        public GreyScaleHistogram(Image image, Channel channel)
        {
            Bitmap bm = (Bitmap)image;

            for (int x = 0; x < bm.Width; x++)
            {
                for (int y = 0; y < bm.Height; y++)
                {
                    Color c = bm.GetPixel(x, y);
                    byte b = 0;
                    switch (channel)
                    {
                        case Channel.R:
                            b = c.R;
                            break;

                        case Channel.G:
                            b = c.G;
                            break;

                        case Channel.B:
                            b = c.B;
                            break;

                        default:
                            throw new ArgumentException($"Unknown channel specification: {channel}");
                    }

                    if (_histo.TryGetValue(b, out int count))
                    {
                        _histo[b] = count + 1;
                    }
                    else
                    {
                        _histo.Add(b, 1);
                    }
                }
            }
        }

        public KeyValuePair<byte, int> Max()
        {
            return _histo.OrderBy(pair => pair.Value).Last();
        }

        public byte Mean() => Mean(_histo);

        public byte Mean(IEnumerable<KeyValuePair<byte, int>> pairs)
        {
            int nValues = 0;
            long sum = 0;

            foreach (var pair in pairs)
            {
                nValues += pair.Value;
                sum += pair.Key * pair.Value;
            }

            return (byte)(sum / nValues);
        }

        // Returns a mean calculated only from the indicated fraction of the highest frequency bins.
        public byte MeanOfNPercentile(float nFrac = 1.0f)
        {
            var nBins = Math.Max(1, (int)(_histo.Count * nFrac));
            var ordered = _histo.OrderByDescending(pair => pair.Value).Take(nBins).ToArray();
            return Mean(ordered);
        }
    }
}
