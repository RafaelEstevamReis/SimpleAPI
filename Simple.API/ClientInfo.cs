using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Simple.API
{
    /// <summary>
    /// Simple json-based api client
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// Response content event
        /// </summary>
        public event EventHandler<ResponseReceived> ResponseDataReceived;

        private readonly HttpClient httpClient;
        /// <summary>
        /// Base url of the API
        /// </summary>
        public readonly Uri BaseUri;
        /// <summary>
        /// Creates a new insance
        /// </summary>
        /// <param name="baseUrl">Base url of the API</param>
        /// <param name="clientHandler">Optional HttpClientHandler to configure</param>
        public ClientInfo(string baseUrl, HttpClientHandler clientHandler = null)
        {
            if (!baseUrl.EndsWith("/")) baseUrl += '/';
            BaseUri = new Uri(baseUrl);

            if (clientHandler == null) clientHandler = new HttpClientHandler();
            clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            httpClient = new HttpClient(clientHandler);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        }
        /// <summary>
        /// Configures the underlying HttpClient
        /// </summary>
        public void ConfigureHttpClient(Action<HttpClient> client) => client(httpClient);

        /// <summary>
        /// Set Authorization header
        /// </summary>
        public void SetAuthorization(string auth)
            => SetHeader("Authorization", auth);
        /// <summary>
        /// Set Authorization header with a Bearer token
        /// </summary>
        public void SetAuthorizationBearer(string token)
            => SetHeader("Authorization", "Bearer " + token);

        /// <summary>
        /// Remove Authorization header
        /// </summary>
        public void RemoveAuthorization()
        {
            lock (httpClient)
            {
                httpClient.DefaultRequestHeaders.Remove("Authorization");
            }
        }

        /// <summary>
        /// Set DefaultRequestHeaders
        /// </summary>
        public void SetHeader(string key, string value)
        {
            lock (httpClient)
            {
                httpClient.DefaultRequestHeaders.Remove(key);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
            }
        }

        /* GET */
        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        public async Task<Response<T>> GetAsync<T>(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            return await processResponseAsync<T>(uri, response);
        }

        /* DELETE */
        /// <summary>
        /// Sends a Delete request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        public async Task<Response> DeleteAsync(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Delete, uri);
            using var response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            return Response.Build(response);
        }

        /* POST */
        /// <summary>
        /// Sends a Post request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response<T>> PostAsync<T>(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, buildContent(value));
            return await processResponseAsync<T>(uri, response);
        }

        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        public async Task<Response> PostAsync(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, null);
            return Response.Build(response);
        }
        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PostAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, buildContent(value));
            return Response.Build(response);
        }

        /// <summary>
        /// Sends a Post request with specified content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="content">Content to be sent</param>
        public async Task<Response<T>> PostAsync<T>(string service, HttpContent content)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, content);
            return await processResponseAsync<T>(uri, response);
        }
        /// <summary>
        /// Sends a Post request with Multipart Form Data content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>

        public async Task<Response<T>> MultipartFormPostAsync<T>(string service, Dictionary<string, string> fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var pair in fields) lst.Add(new KeyValuePair<string, string>(pair.Key, pair.Value));
            return await MultipartFormPostAsync<T>(service, lst);
        }
        /// <summary>
        /// Sends a Post request with Multipart Form Data content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public async Task<Response<T>> MultipartFormPostAsync<T>(string service, NameValueCollection fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var k in fields.AllKeys) lst.Add(new KeyValuePair<string, string>(k, fields[k]));
            return await MultipartFormPostAsync<T>(service, lst);
        }
        /// <summary>
        /// Sends a Post request with Multipart Form Data content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public async Task<Response<T>> MultipartFormPostAsync<T>(string service, IEnumerable<KeyValuePair<string, string>> fields)
        {
            var formContent = new MultipartFormDataContent();
            foreach (var item in fields)
            {
                formContent.Add(new StringContent(item.Value), item.Key);
            }

            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, formContent);
            return await processResponseAsync<T>(uri, response);
        }

        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public async Task<Response<T>> FormUrlEncodedPostAsync<T>(string service, Dictionary<string, string> fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var pair in fields) lst.Add(new KeyValuePair<string, string>(pair.Key, pair.Value));
            return await FormUrlEncodedPostAsync<T>(service, lst);
        }
        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public async Task<Response<T>> FormUrlEncodedPostAsync<T>(string service, NameValueCollection fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var k in fields.AllKeys) lst.Add(new KeyValuePair<string, string>(k, fields[k]));
            return await FormUrlEncodedPostAsync<T>(service, lst);
        }
        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public async Task<Response<T>> FormUrlEncodedPostAsync<T>(string service, IEnumerable<KeyValuePair<string, string>> fields)
        {
            var formContent = new FormUrlEncodedContent(fields);

            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, formContent);
            return await processResponseAsync<T>(uri, response);
        }
        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="values">Object with fields to be mapped</param>
        public async Task<Response<T>> FormUrlEncodedPostAsync<T>(string service, object values)
            => await FormUrlEncodedPostAsync<T>(service, Helper.buildParams(values));

        /* PUT */
        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PutAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Put, uri);
            msg.Content = buildContent(value);
            using var response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
            return Response.Build(response);
        }

        /* PATCH */
        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PatchAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);

            using var msg = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            msg.Content = buildContent(value);
            using var response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);

            return Response.Build(response);
        }

        /* OPTIONS */
        /// <summary>
        /// Sends an Options request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Object with fields to be mapped</param>
        public async Task<Response> OptionsAsync(string service, object value)
            => await OptionsAsync(service, Helper.buildParams(value));

#if !NETSTANDARD1_1
        /// <summary>
        /// Sends an Options request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="headers">Headers to be sent</param>
        public async Task<Response> OptionsAsync(string service, IEnumerable<(string, string)> headers)
        {
            var kp = headers.Select(p => new KeyValuePair<string, string>(p.Item1, p.Item2));
            return await OptionsAsync(service, kp);
        }
#endif

        /// <summary>
        /// Sends an Options request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="headers">Headers to be sent</param>
        public async Task<Response> OptionsAsync(string service, IEnumerable<KeyValuePair<string, string>> headers)
        {
            var uri = new Uri(BaseUri, service);

            var message = new HttpRequestMessage(HttpMethod.Options, uri);

            foreach (var pair in headers)
            {
                message.Headers.Add(pair.Key, pair.Value);
            }

            var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            return Response.Build(response);
        }

        private HttpContent buildContent(object value)
        {
            var jsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return new StringContent(jsonValue, Encoding.UTF8, "application/json");
        }
        private async Task<Response<T>> processResponseAsync<T>(Uri uri, HttpResponseMessage response)
        {
            T data = default;

            bool binary = typeof(T) == typeof(byte[]);

            string content = null;
            if (binary) data = (T)(object)await response.Content.ReadAsByteArrayAsync();
            else content = await response.Content.ReadAsStringAsync();

            string errorData = null;

            var contentHeaders = response.Content.Headers;

            ResponseDataReceived?.Invoke(this, new ResponseReceived()
            {
                Received = DateTime.UtcNow,
                Uri = uri,
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Content = content,
            });

            if (!response.IsSuccessStatusCode)
            {
                errorData = content;
            }
            else if (!binary)
            {
                // Can be encoded 
                if (content.StartsWith("%7B"))
                {
                    // URLEncoded json
                    content = WebUtility.UrlDecode(content);
                }

                if (typeof(T) == typeof(string)) data = (T)(object)content;
                else if (typeof(T) == typeof(JWT))
                {
                    if (content.Contains("\""))
                    {
                        data = (T)(object)JWT.Parse(content.Replace("\"", ""));
                    }
                    else
                    {
                        data = (T)(object)JWT.Parse(content);
                    }
                }
                else data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);

            }

            var d = Response<T>.Build(response, contentHeaders, data, errorData);

            return d;
        }

        /// <summary>
        /// Response received data
        /// </summary>
        public class ResponseReceived
        {
            /// <summary>
            /// DateTime of the response
            /// </summary>
            public DateTime Received { get; set; }
            /// <summary>
            /// Request uri
            /// </summary>
            public Uri Uri { get; set; }
            /// <summary>
            /// Response content
            /// </summary>
            public string Content { get; set; }
            /// <summary>
            /// Successful request
            /// </summary>
            public bool Success { get; set; }
            /// <summary>
            /// StatusCode of the response
            /// </summary>
            public int StatusCode { get; set; }

            /// <summary>
            /// Returns a string that represents the current object
            /// </summary>
            public override string ToString()
            {
                return $"{Received:G} {Uri.PathAndQuery} [{StatusCode}] {Content}";
            }
        }
    }
}
