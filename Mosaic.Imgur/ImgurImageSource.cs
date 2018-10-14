using Mosaic.Interfaces.ImageSource;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mosaic.Imgur
{
    public class ImgurImageSource : IImageSource
    {
        public ImgurImageSource()
        {
        }

        public Task<IEnumerable<IImage>> GetImages(IImageQueryResolver queryResolver, IImageQuery query, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var resolver = new ImgurQueryResolver();
                var queryResult = await resolver.Resolve(new ImgurQuery("dogs", "png"), cancelToken);

                return new List<IImage>() as IEnumerable<IImage>;
            });
        }
    }
}
