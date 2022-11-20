using System;
using System.Net;
using System.Threading.Tasks;

namespace SV.Maat.lib
{
    public static class Downloader
    {
        public static async Task<byte[]> Download(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.AllowAutoRedirect = true;
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                byte[] data = new byte[response.ContentLength];
                var stream = response.GetResponseStream();

                for (int ii = 0; ii < response.ContentLength; ii += 1)
                {
                    stream.Read(data, ii, 1);
                }

                return data;
            }


        }

        public static async Task<(byte[], string)> DownloadWithType(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.AllowAutoRedirect = true;
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    return (null, "");
                }

                byte[] data = new byte[response.ContentLength];
                string contentType = response.ContentType;
                var stream = response.GetResponseStream();

                for (int ii = 0; ii < response.ContentLength; ii += 1)
                {
                    stream.Read(data, ii, 1);
                }

                return (data, contentType);
            }


        }
    }
}
