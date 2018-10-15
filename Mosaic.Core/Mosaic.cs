using Mosaic.Interfaces.ImageSource;
using Mosaic.Interfaces.Processing;
using Mosaic.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
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

                return await CreateCompletedImage();
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
            if (_completedCells[row, col] == null)
            {
                _completedCells[row, col] = cellImage;
                _completedCellCount++;
            }
        }

        public Task<Image> CreateCompletedImage()
        {
            return Task.Run(() =>
            {
                var completedImage = new Bitmap(TargetImageSize.Width, TargetImageSize.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(completedImage))
                {
                    for (int r = 0; r < GridSize.Height; r++)
                    {
                        for (int c = 0; c < GridSize.Width; c++)
                        {
                            var cellImage = _completedCells[r, c] ?? GetTargetCell(r, c).Image;
                            g.DrawImage(
                                cellImage,
                                new Rectangle(CellSize.Width * c, CellSize.Height * r, CellSize.Width, CellSize.Height),
                                new Rectangle(0, 0, cellImage.Width, cellImage.Height),
                                GraphicsUnit.Pixel);
                        }
                    }
                }

                return completedImage as Image;
            });
        }

        private Task InitCells(IImageFeatureExtractor featureExtractor)
        {
            return Task.Run(() =>
            {
                _completedCells = new Image[GridSize.Height, GridSize.Width];
                _targetCellFeatures = new IImageFeatureSet[GridSize.Height, GridSize.Width];

                // subdivide the target image into grid of (rows x cols) cells and extract features for each
                var resizedTarget = ImageUtils.MakeGrayscale(new Bitmap(2, 2, PixelFormat.Format24bppRgb), TargetImageSize);
                
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
                while (!cancelToken.IsCancellationRequested)
                {
                    var nextImage = await imageSource.GetNextImage(cancelToken);
                    if (nextImage != null)
                    {
                        var nextImageReduced = ImageUtils.MakeGrayscale(nextImage.RawImage, CellSize);
                        var features = featureExtractor.ExtractFeatures(nextImageReduced);

                        bool isMatch = false;
                        for (int r = 0; r < GridSize.Height; r++)
                        {
                            if (isMatch)
                            {
                                break;
                            }

                            for (int c = 0; c < GridSize.Width; c++)
                            {
                                if (GetCompletedCell(r, c) == null && features.IsMatch(GetTargetCell(r, c)))
                                {
                                    SetCompletedCell(r, c, nextImageReduced);
                                    //isMatch = true;

                                    var remaining = GridSize.Width * GridSize.Height - _completedCellCount;
                                    System.Console.WriteLine($"!! MATCH found for image id: '{nextImage.Id}' for target cell r:{r}, c:{c}.  Remaining: {remaining}");
                                    break;
                                }
                            }
                        }

                        nextImage.RawImage.Dispose();
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
