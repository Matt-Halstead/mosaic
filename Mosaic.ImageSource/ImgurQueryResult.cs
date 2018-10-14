using Mosaic.Interfaces.ImageSource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Mosaic.ImageSource
{
    public class ImgurQueryResult : IImageQueryResult
    {
        private readonly List<IImage> _images = new List<IImage>();

        public void Add(IImage image)
        {
            if (image == null)
            {
                throw new ArgumentException(nameof(image));
            }

            _images.Add(image);
        }

        public IEnumerable<IImage> Images => _images;

        public void Parse(JObject json)
        {
            System.Console.WriteLine($"Parsing response:\n{json.ToString()}");
        }
    }
}
