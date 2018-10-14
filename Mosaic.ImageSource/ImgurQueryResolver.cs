using Mosaic.Interfaces.ImageSource;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Mosaic.ImageSource
{
    internal class ImgurImage : IImage
    {
        public Image RawImage => new Bitmap(10, 10, PixelFormat.Format32bppArgb);
    }

    public class ImgurQueryResolver : IImageQueryResolver
    {
        private const string ImgurClientToken = "Client-ID 6a3bd1babe3bb8c";

        public Task<IImageQueryResult> Resolve(IImageQuery imageQuery, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var result = new ImgurQueryResult();
                
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var url = @"https://api.imgur.com/3/gallery/search/top//1";
                        var query = FormatQueryString(Tuple.Create("q", imageQuery.GetFormattedParams()));
                        client.DefaultRequestHeaders.Add("Authorization", ImgurClientToken);

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

        private async void DownloadShelvesetContent(string shelvesetId, string json, string personalAccessToken)
        {
            //Console.WriteLine(json);

            var shelvesetFolder = $@"C:\Source\_shelvesets\{shelvesetId}";

            if (JsonConvert.DeserializeObject(json) is JObject jObject)
            {
                if (jObject.TryGetValue("value", out JToken jValue))
                {
                    var sb = new StringBuilder();

                    using (HttpClient client = new HttpClient())
                    {
                        string personalAccessTokenBase64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", personalAccessTokenBase64);

                        foreach (var jTokenValue in jValue.ToArray())
                        {
                            string changeType = jTokenValue["changeType"].ToString();

                            var item = jTokenValue["item"];
                            var version = item["version"];
                            var path = item["path"].ToString();
                            var url = item["url"].ToString();

                            sb.AppendLine($"{changeType}: {path}");

                            if (new[] { "add", "edit" }.Contains(changeType))
                            {
                                var destPath = path.Replace('/', '\\').Replace("$", shelvesetFolder);

                                EnsureFolderExists(Path.GetDirectoryName(destPath));

                                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                                {
                                    using (
                                        Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                                        stream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                    {
                                        await contentStream.CopyToAsync(stream);
                                    }
                                }
                            }
                        }

                        File.WriteAllText(Path.Combine(shelvesetFolder, "shelveset_info.txt"), sb.ToString());
                    }
                }
            }
        }

        private string FormatQueryString(params Tuple<string, string>[] queryArgs)
        {
            var queryString = string.Join("&", queryArgs.Select(query => $"{query.Item1}={HttpUtility.UrlEncode(query.Item2)}"));
            queryString = string.IsNullOrEmpty(queryString) ? "" : $"?{queryString}";
            return queryString;
        }

        private void EnsureFolderExists(string path)
        {
            var folders = path.Split('\\');
            var current = string.Empty;
            foreach (var folder in folders)
            {
                current = Path.Combine(current, folder);
                if (current.EndsWith(":"))
                {
                    current += "\\";
                }

                if (!Directory.Exists(current))
                {
                    Directory.CreateDirectory(current);
                }
            }
        }
    }
}
