using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Simple.API
{
#if NETSTANDARD1_1
    public static class ExtensionNetStd11
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri uri, HttpContent content)
        {
            var req = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            req.Content = content;

            return await client.SendAsync(req);
        }
    }
#endif
}