using Mosaic.Core;
using Mosaic.ImageSource;
using Mosaic.Interfaces.ImageSource;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Console
{
    class Program
    {
        private static readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private static readonly AutoResetEvent _completedEvent = new AutoResetEvent(false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            InitIoC();

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
                var resolver = IoC.Resolve<IImageQueryResolver>();
                var queryResult = await resolver.Resolve(new ImgurQuery("dogs", "png"), cancelToken);

                int w = queryResult.Images.FirstOrDefault()?.RawImage.Width ?? 0;

                _completedEvent.Set();
            });
        }

        private static void InitIoC()
        {
            IoC.RegisterInstance<IImageQueryResolver>(new ImgurQueryResolver());
        }
    }
}
