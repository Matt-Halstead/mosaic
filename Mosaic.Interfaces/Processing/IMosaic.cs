using Mosaic.Interfaces.ImageSource;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Interfaces.Processing
{
    public interface IMosaic
    {
        Task<Image> Build(IImageSource imageSource, IImageFeatureExtractor featureExtractor, CancellationToken cancelToken);

        Image TargetImage { get; }
        Size TargetImageSize { get; }
        Size GridSize { get; }

        bool IsComplete { get; }

        IImageFeatureSet GetTargetCell(int row, int col);
        Image GetCompletedCell(int row, int col);
        void SetCompletedCell(int row, int col, Image cellImage);

        Image GetCompletedImage();
    }
}
