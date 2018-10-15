using Mosaic.Interfaces.ImageSource;
using Mosaic.Interfaces.Processing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Core
{
    public class Mosaic : IMosaic
    {
        private IImageFeatureSet[,] _targetCellFeatures;
        private Image[,] _completedCells;
        private int _completedCellCount = 0;

        public Mosaic(Image target, Size targetImageSize, Size gridSize)
        {
            TargetImage = target;

            if (target == null || (target.Width * target.Height) == 0)
            {
                throw new ArgumentException("target image is null or invalid size.");
            }

            CellSize = new Size(targetImageSize.Width / gridSize.Width, targetImageSize.Height / gridSize.Height);
            TargetImageSize = new Size(CellSize.Width * gridSize.Width, CellSize.Height * gridSize.Height);
            GridSize = gridSize;

            if (GridSize.Width * GridSize.Height <= 0)
            {
                throw new ArgumentException("Grid width and height must be positive.");
            }

            if (TargetImageSize.Width * TargetImageSize.Height <= 0)
            {
                throw new ArgumentException("Target image width and height must be positive.");
            }

            var intCellsX = TargetImageSize.Width / GridSize.Width;
            var intCellsY = TargetImageSize.Height / GridSize.Height;
            if ((intCellsX * GridSize.Width < TargetImageSize.Width) ||
                (intCellsX * GridSize.Width < TargetImageSize.Width))
            {
                throw new ArgumentException("Target image width and height be a multiple of the cell size.");
            }
        }

        public Task<Image> Build(IImageSource imageSource, IImageFeatureExtractor featureExtractor, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                await InitCells(featureExtractor);
                await AssignImagesToCells(imageSource, featureExtractor, cancelToken);

                return GetCompletedImage();
            });
        }

        public Image TargetImage { get; private set; }
        public Size TargetImageSize { get; private set; }
        public Size GridSize { get; private set; }
        public Size CellSize { get; private set; }

        public bool IsComplete => _completedCellCount == (GridSize.Width * GridSize.Height);

        public IImageFeatureSet GetTargetCell(int row, int col)
        {
            return _targetCellFeatures[row, col];
        }

        public Image GetCompletedCell(int row, int col)
        {
            return _completedCells[row, col];
        }

        public void SetCompletedCell(int row, int col, Image cellImage)
        {
            var isSet = _completedCells[row, col] == null;
            _completedCells[row, col] = cellImage;

            if (!isSet)
            {
                _completedCellCount++;
            }
        }

        public Image GetCompletedImage()
        {
            var completedImage = new Bitmap(TargetImageSize.Width, TargetImageSize.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(completedImage))
            {
                for (int r = 0; r < GridSize.Height; r++)
                {
                    for (int c = 0; c < GridSize.Width; c++)
                    {
                        var cellImage = _completedCells[r, c] ?? GetTargetCell(r, c).Image;
                        g.DrawImage(cellImage, CellSize.Width * c, CellSize.Height * r, cellImage.Width, cellImage.Height);
                    }
                }
            }

            return completedImage;
        }

        private Task InitCells(IImageFeatureExtractor featureExtractor)
        {
            return Task.Run(() =>
            {
                _completedCells = new Image[GridSize.Height, GridSize.Width];
                _targetCellFeatures = new IImageFeatureSet[GridSize.Height, GridSize.Width];

                // subdivide the target image into grid of (rows x cols) cells and extract features for each
                var resizedTarget = new Bitmap(TargetImageSize.Width, TargetImageSize.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(resizedTarget))
                {
                    g.DrawImage(TargetImage, 0, 0, TargetImageSize.Width, TargetImageSize.Height);
                }

                for (int r = 0; r < GridSize.Height; r++)
                {
                    for (int c = 0; c < GridSize.Width; c++)
                    {
                        var imageRect = new Rectangle(new Point(CellSize.Width * c, CellSize.Height * r), CellSize);
                        var cellImage = resizedTarget.Clone(imageRect, PixelFormat.Format24bppRgb);
                        _targetCellFeatures[r, c] = featureExtractor.ExtractFeatures(cellImage);
                    }
                }

                resizedTarget.Dispose();
            });
        }

        private Task AssignImagesToCells(IImageSource imageSource, IImageFeatureExtractor featureExtractor, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var cells = (from r in Enumerable.Range(0, GridSize.Height)
                             from c in Enumerable.Range(0, GridSize.Width)
                             select new { r, c } )
                             .ToList();
                    

                while (!cancelToken.IsCancellationRequested)
                {
                    var nextImage = await imageSource.GetNextImage(cancelToken);
                    if (nextImage != null)
                    {
                        var features = featureExtractor.ExtractFeatures(nextImage.RawImage);

                        var remaining = cells.Where(cell => GetCompletedCell(cell.r, cell.c) != null).ToList();
                        foreach (var cell in remaining)
                        {
                            if (features.IsMatch(GetTargetCell(cell.r, cell.c)))
                            {
                                SetCompletedCell(cell.r, cell.c, features.Image);
                                break;
                            }
                        }
                    }
                    else
                    {
                        // hacky, fix this up
                        System.Console.WriteLine("ERROR: Failed to retreive any images, quitting...");
                        break;
                    }
                }
            });
        }        
    }
}
