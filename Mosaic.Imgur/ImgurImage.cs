using Mosaic.Interfaces.ImageSource;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mosaic.Imgur
{
    internal class ImgurImage : IImage
    {
        public ImgurImage(string id, string title, string contentType, Uri link, IEnumerable<string> tags)
        {
            Id = id;
            Title = title;
            ContentType = contentType;
            Link = link;
            Tags.AddRange(tags);
        }

        public Task<IImage> LoadImage()
        {
            return Task.Run(async () =>
            {
                HttpClient client = new HttpClient();
                client.MaxResponseContentBufferSize = 256000;
                Image img = Image.FromStream(await client.GetStreamAsync(Link));

                RawImage = img;

                System.Console.WriteLine($"Downloaded image from url: {Link}");
                DumpToTempFile(this);

                return this as IImage;
            });
        }

        public Image RawImage { get; private set; }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public string ContentType { get; private set; }
        public List<string> Tags { get; private set; } = new List<string>();
        public Uri Link { get; private set; }

        // Todo, copy pasta from Program.cs
        private static void DumpToTempFile(IImage image)
        {
            var tempFile = Path.GetTempFileName();
            image.RawImage.Save(tempFile, ImageFormat.Png);

            var targetFolder = Path.Combine(Path.GetTempPath(), "mosaic", "cells");
            Directory.CreateDirectory(targetFolder);

            var filename = Path.Combine(targetFolder, $"img-{image.Id}.png");

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            File.Move(tempFile, filename);

            System.Console.WriteLine($"Wrote image to temp file: {filename}");
        }

    }
}
