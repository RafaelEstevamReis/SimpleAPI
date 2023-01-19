#if NETSTANDARD
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.API
{
    public static class ExtensionNetStd11
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri uri, HttpContent content)
        {
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            req.Content = content;

            return await client.SendAsync(req);
        }
    }
}
#endif
