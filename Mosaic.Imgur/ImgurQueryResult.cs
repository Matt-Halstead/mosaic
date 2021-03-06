﻿using Mosaic.Interfaces.ImageSource;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mosaic.Imgur
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

        public List<IImage> Images => _images;

        public void Parse(JObject json)
        {
            //System.Console.WriteLine($"Parsing response:\n{json.ToString()}");

            var excluded = new string[] { "image/gif", "video/mp4" };

            if (json != null)
            {
                foreach (var child in json["data"].Children())
                {
                    bool albumIsNsfw = child["nsfw"].Value<bool>();
                    bool albumIsAd = child["is_ad"].Value<bool>();
                    if (!albumIsNsfw && !albumIsAd)
                    {
                        string albumId = child["id"].Value<string>();
                        string albumTitle = child["title"].Value<string>();

                        JToken imageDefs = child["images"];
                        if (imageDefs != null)
                        {
                            foreach (var imageDef in imageDefs.Children())
                            {
                                var type = imageDef["type"].Value<string>();
                                if (!excluded.Contains(type, StringComparer.OrdinalIgnoreCase) &&
                                    imageDef["width"].Value<int>() < 2000 &&
                                    imageDef["height"].Value<int>() < 2000)
                                {
                                    var image = new ImgurImage(
                                        imageDef["id"].Value<string>() ?? albumId,
                                        imageDef["title"].Value<string>() ?? albumTitle,
                                        type,
                                        new Uri(imageDef["link"].Value<string>()),
                                        child["tags"].Children()["name"].Values<string>());

                                    Images.Add(image);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
