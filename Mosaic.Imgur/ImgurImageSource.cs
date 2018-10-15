using Mosaic.Interfaces.ImageSource;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Imgur
{
    public class ImgurImageSource : IImageSource
    {
        private readonly IImageQuery _query;
        private readonly IImageQueryResolver _queryResolver;

        private ConcurrentQueue<IImage> _queuedDownloads = new ConcurrentQueue<IImage>();
        private ConcurrentQueue<IImage> _completedDownloads = new ConcurrentQueue<IImage>();

        public ImgurImageSource(IImageQuery query, CancellationToken cancelToken)
        {
            _query = query;
            _queryResolver = new ImgurQueryResolver();

            StartDownloadingImages(cancelToken);
        }

        public Task<IImage> GetNextImage(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    // kick off next downloads
                    if (_queuedDownloads.TryDequeue(out IImage nextImage))
                    {
                        _completedDownloads.Enqueue(await nextImage.LoadImage());
                    }

                    if (_completedDownloads.TryDequeue(out IImage image))
                    {
                        return image;
                    }

                    Thread.Sleep(100);
                }

                System.Console.WriteLine("There are no more images to download.");
                return null;
            });
        }

        private Task StartDownloadingImages(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (!_queuedDownloads.TryPeek(out IImage image))
                    {
                        await GetNextBatchOfImages(cancelToken);
                    }

                    Thread.Sleep(5000);
                }

                System.Console.WriteLine("STOPPED downloading images.");
            });
        }

        private Task GetNextBatchOfImages(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var queryResult = await _queryResolver.ExecuteQuery(_query, cancelToken);

                foreach (var image in queryResult.Images)
                {
                    _queuedDownloads.Enqueue(image);
                }
            });
        }
    }
}
