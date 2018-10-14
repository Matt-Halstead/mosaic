using Mosaic.Interfaces.ImageSource;
using Mosaic.Interfaces.Processing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Mosaic.Core
{
    public class Mosaic : IMosaic
    {
        private IImageFeatureSet[,] _targetCellFeatures;
        private Image[,] _completedCells;

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

        public Task<Image> Build(IImageSource imageSource, IImageFeatureExtractor featureExtractor)
        {
            return Task.Run(async () =>
            {
                await InitCells(featureExtractor);
                await AssignImagesToCells(imageSource, featureExtractor);

                return GetCompletedImage();
            });
        }

        public Image TargetImage { get; private set; }
        public Size TargetImageSize { get; private set; }
        public Size GridSize { get; private set; }
        public Size CellSize { get; private set; }

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
            _completedCells[row, col] = cellImage;
        }

        public Image GetCompletedImage()
        {
            throw new NotImplementedException();
        }

        private Task InitCells(IImageFeatureExtractor featureExtractor)
        {
            return Task.Run(() =>
            {
                _completedCells = new Image[GridSize.Height, GridSize.Width];
                _targetCellFeatures = new IImageFeatureSet[GridSize.Height, GridSize.Width];

                // subdivide the target image into grid of (rows x cols) cells and extract features for each
                var resizedTarget = new Bitmap(TargetImageSize.Width, TargetImageSize.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(TargetImage))
                {
                    g.DrawImage(TargetImage, 0, 0, TargetImageSize.Width, TargetImageSize.Height);
                }

                for (int r = 0; r < GridSize.Height; r++)
                {
                    for (int c = 0; c < GridSize.Width; c++)
                    {
                        var imageRect = new Rectangle(new Point(CellSize.Width * c, CellSize.Height * r), CellSize);
                        var lockedBits = resizedTarget.LockBits(imageRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        var cellImage = resizedTarget.Clone(imageRect, PixelFormat.Format24bppRgb);
                        _targetCellFeatures[r, c] = featureExtractor.ExtractFeatures(cellImage);
                        resizedTarget.UnlockBits(lockedBits);
                    }
                }
            });
        }

        private Task AssignImagesToCells(IImageSource imageSource, IImageFeatureExtractor featureExtractor)
        {
            return Task.Run(() =>
            {
                var whiteBrush = new SolidBrush(Color.White);
                var blackBrush = new SolidBrush(Color.Black);

                for (int r = 0; r < GridSize.Height; r++)
                {
                    for (int c = 0; c < GridSize.Width; c++)
                    {
                        //_completedCells[r, c] = _targetCellFeatures[r, c].Image.Clone() as Image;

                        // hack to init the image!
                        using (var g = Graphics.FromImage(_completedCells[r, c]))
                        {
                            var brush = ((c + r * c) % 2) == 0 ? whiteBrush : blackBrush;
                            g.FillRectangle(brush, new Rectangle(new Point(0, 0), CellSize));

                            System.Threading.Thread.Sleep(40);
                        }
                    }
                }

                whiteBrush.Dispose();
                blackBrush.Dispose();
            });
        }
    }
}
