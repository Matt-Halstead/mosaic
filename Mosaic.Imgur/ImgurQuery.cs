using Mosaic.Interfaces.ImageSource;

namespace Mosaic.Imgur
{
    public class ImgurQuery : IImageQuery
    {
        public ImgurQuery(string queryString)
        {
            QueryString = queryString;
        }

        public string QueryString { get; private set; }
    }
}
