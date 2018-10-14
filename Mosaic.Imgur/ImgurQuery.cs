using Mosaic.Interfaces.ImageSource;

namespace Mosaic.Imgur
{
    public class ImgurQuery : IImageQuery
    {
        public ImgurQuery(string title, string ext)
        {
            Title = title;
            Extension = ext;
        }

        public string Title { get; private set; }
        public string Extension { get; private set; }

        public string GetFormattedParams() => $"title: {Title} ext: {Extension}";
    }
}
