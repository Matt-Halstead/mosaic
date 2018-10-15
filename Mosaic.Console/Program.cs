using Mosaic.Core;
using Mosaic.Imgur;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Console
{
    class Program
    {
        private static readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private static readonly AutoResetEvent _completedEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Start(_cancellation.Token);
            WaitForCancellationOrCompletion().Wait();

            System.Console.WriteLine("Done.");

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Console.WriteLine("Press any key...");
                System.Console.ReadKey();
            }
        }

        private static Task WaitForCancellationOrCompletion()
        {
            return Task.Run(() =>
            {
                System.Console.WriteLine("----- Press ESC to cancel processing! -----");

                while (!_cancellation.IsCancellationRequested)
                {
                    if (System.Console.KeyAvailable && System.Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        _cancellation.Cancel();
                        System.Console.WriteLine("User cancelled!");
                        break;
                    }

                    if (_completedEvent.WaitOne(200))
                    {
                        System.Console.WriteLine("Processing completed!");
                        break;
                    }
                }
            });
        }

        private static Task Start(CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var targetSize = new Size(1920, 1080);
                var gridSize = new Size(64, 40);
                Image targetImage = null;

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Mosaic.Console.TestImage.png";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    targetImage = Image.FromStream(stream);
                }

                var mosaic = new Mosaic.Core.Mosaic(targetImage, targetSize, gridSize);
                var imageSource = new ImgurImageSource(new ImgurQuery("dogs", "png"), cancelToken);
                var featureExtractor = new BasicImageFeatureExtractor();
                var resultImage = await mosaic.Build(imageSource, featureExtractor, cancelToken);

                DumpToTempFile(resultImage);

                _completedEvent.Set();
            });
        }

        private static void DumpToTempFile(Image resultImage)
        {
            var tempFile = Path.GetTempFileName();
            resultImage.Save(tempFile, ImageFormat.Png);

            var targetFolder = Path.Combine(Path.GetTempPath(), "mosaic");
            Directory.CreateDirectory(targetFolder);

            var filename = Path.Combine(targetFolder, $"Mosaic-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.png");
            File.Move(tempFile, filename);

            System.Console.WriteLine($"Wrote image to temp file: {filename}");
        }
    }
}
