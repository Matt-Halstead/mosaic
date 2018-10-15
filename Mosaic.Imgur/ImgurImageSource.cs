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
            return Task.Run(() =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    IImage image;
                    if (_completedDownloads.TryDequeue(out image))
                    {
                        return image;
                    }

                    Thread.Sleep(200);
                }

                System.Console.WriteLine("There are no more images to donwload.");
                return null;
            });
        }

        private Task StartDownloadingImages(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (_queuedDownloads.TryDequeue(out IImage image))
                    {
                        _completedDownloads.Enqueue(await image.LoadImage());
                    }
                    else
                    {
                        await GetNextBatchOfImages(cancelToken);
                        Thread.Sleep(250);
                    }
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
