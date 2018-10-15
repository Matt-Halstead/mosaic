using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Mosaic.Util
{
    public static class ImageUtils
    {
        public static Bitmap MakeGrayscale(Image original, Size targetImageSize)
        {
            var newBitmap = new Bitmap(targetImageSize.Width, targetImageSize.Height);

            var colorMatrix = new ColorMatrix(
               new float[][]
               {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
               });

            var attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            using (var g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(original, new Rectangle(0, 0, targetImageSize.Width, targetImageSize.Height),
                    0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }

            return newBitmap;
        }

        public static void SaveImageToFile(Image image, string filename)
        {
            string folder = Path.GetDirectoryName(filename);
            FileUtils.EnsureFolderExists(folder);
            image.Save(filename, ImageFormat.Png);
        }
    }
}
