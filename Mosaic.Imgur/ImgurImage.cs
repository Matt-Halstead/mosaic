using Mosaic.Interfaces.ImageSource;
using Mosaic.Util;
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
                try
                {
                    var tempFolder = Path.Combine(Path.GetTempPath(), "mosaic", "cells");
                    var downloadPath = Path.Combine(tempFolder, $"{Id}.png");

                    if (!File.Exists(downloadPath))
                    {
                        HttpClient client = new HttpClient();
                        client.MaxResponseContentBufferSize = 256000;
                        using (var stream = await client.GetStreamAsync(Link))
                        {
                            Image img = Image.FromStream(stream);
                            RawImage = img;
                        }

                        System.Console.WriteLine($"DOWNLOADED {Link} --> {downloadPath}");
                        ImageUtils.SaveImageToFile(RawImage, tempFolder, $"{Id}.png");
                    }

                    RawImage = Image.FromFile(downloadPath);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"Error getting image {Id}: {e.Message}");
                    RawImage = new Bitmap(10, 10, PixelFormat.Format24bppRgb);
                }

                return this as IImage;
            });
        }

        public Image RawImage { get; private set; }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public string ContentType { get; private set; }
        public List<string> Tags { get; private set; } = new List<string>();
        public Uri Link { get; private set; }
    }
}
