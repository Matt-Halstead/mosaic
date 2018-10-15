using Mosaic.Interfaces.ImageSource;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Mosaic.Imgur
{
    internal class ImgurQueryResolver : IImageQueryResolver
    {
        public const string ImgurClientToken = "Client-ID 6a3bd1babe3bb8c";

        private int _page = 1;

        public Task<IImageQueryResult> ExecuteQuery(IImageQuery imageQuery, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var result = new ImgurQueryResult();
                
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //var url = @"https://api.imgur.com/3/gallery/search/top//1";
                        var url = $@"https://api.imgur.com/3/gallery/search//{_page}";
                        var query = FormatQueryString(Tuple.Create("q", imageQuery.QueryString));
                        client.DefaultRequestHeaders.Add("Authorization", ImgurClientToken);

                        _page++;

                        using (HttpResponseMessage response = await client.GetAsync($"{url}{query}"))
                        {
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();

                            JObject json = JObject.Parse(responseBody);
                            result.Parse(json);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.ToString());
                }

                return result as IImageQueryResult;
            });
        }

        private string FormatQueryString(params Tuple<string, string>[] queryArgs)
        {
            var queryString = string.Join("&", queryArgs.Select(query => $"{query.Item1}={HttpUtility.UrlEncode(query.Item2)}"));
            queryString = string.IsNullOrEmpty(queryString) ? "" : $"?{queryString}";
            return queryString;
        }        
    }
}
