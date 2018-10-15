using Mosaic.Interfaces.ImageSource;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Imgur
{
    // Downloads the results of image query requests sent to Imgur API, caching files locally for faster reuse.
    public class ImgurImageSource : IImageSource
    {
        private readonly IImageQuery _query;
        private readonly IImageQueryResolver _queryResolver;

        private readonly ConcurrentQueue<IImage> _queuedDownloads = new ConcurrentQueue<IImage>();
        private readonly ConcurrentQueue<IImage> _completedDownloads = new ConcurrentQueue<IImage>();

        public ImgurImageSource(IImageQuery query, CancellationToken cancelToken)
        {
            _query = query;
            _queryResolver = new ImgurQueryResolver();

            QueueLoadOfCachedFiles();

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

                return null;
            });
        }

        private void QueueLoadOfCachedFiles()
        {
            // Do an init check for local files.  A bit hacky but much faster.  Needs work!
            var path = Path.Combine(Path.GetTempPath(), "mosaic", "cells");
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.png");

                foreach (var file in files)
                {
                    var fileBaseName = Path.GetFileNameWithoutExtension(file);
                    var newImage = new ImgurImage(fileBaseName, fileBaseName, "image/png", null, new string[0]);
                    _queuedDownloads.Enqueue(newImage);
                    System.Console.WriteLine($"FOUND cached image: {file}");
                }
            }
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

                    Thread.Sleep(1000);
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
