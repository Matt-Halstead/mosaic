using Mosaic.Interfaces.ImageSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Imgur
{
    public class ImgurImageSource : IImageSource
    {
        private readonly IImageQuery _query;
        private readonly IImageQueryResolver _queryResolver;

        private Queue<IImage> _cachedImages = new Queue<IImage>();

        public ImgurImageSource(IImageQuery query = null)
        {
            _query = query ?? new ImgurQuery("dogs", "png");
            _queryResolver = new ImgurQueryResolver();
        }

        public Task<IImage> GetNextImage(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                if (_cachedImages.Count() > 0)
                {
                    return _cachedImages.Dequeue();
                }

                var images = await GetBatchOfImages(cancelToken);
                foreach (var image in images)
                {
                    _cachedImages.Enqueue(image);
                }

                if (_cachedImages.Count() > 0)
                {
                    return _cachedImages.Dequeue();
                }

                return null;
            });
        }

        internal Task<List<IImage>> GetBatchOfImages(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var queryResult = await _queryResolver.Resolve(_query, cancelToken);
                return queryResult.Images.ToList();
            });
        }
    }
}
