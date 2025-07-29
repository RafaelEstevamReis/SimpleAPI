using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Overrides json serialization
        /// </summary>
        public event EventHandler<JsonSerializeArgs> JsonSerializeOverride;
        /// <summary>
        /// HttpRequestMessage ready to be sent
        /// </summary>
        public event EventHandler<HttpRequestMessage> BeforeSend;
        /// <summary>
        /// Overrides deserialization process for each JValue
        /// </summary>
        public event EventHandler<DeserializeJValueOverrideArgs> DeserializeJValueOverride;
        /// <summary>
        /// Overrides deserialization process for the Object before .ToObject
        /// Occurs before DeserializeJValueOverride
        /// </summary>
        public event EventHandler<DeserializeJObjectOverrideArgs> DeserializeJObjectOverride;

        /// <summary>
        /// Ignore Nulls when building parameteres
        /// </summary>
        public bool NullParameterHandlingPolicy_IgnoreNulls = true;

        /// <summary>
        /// Defines the default timeout for new HttpClient instances created by the ClientInfo constructor.
        /// This value is applied only during instance creation. Changing it afterward does not affect existing instances.
        /// </summary>
        public static TimeSpan? GlobalDefaultTimeout = null;

        /// <summary>
        /// Gets or sets the timespan to wait before the request times out.
        /// </summary>
        public TimeSpan Timeout
        {
            get => httpClient.Timeout;
            set => httpClient.Timeout = value;
        }

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
        public ClientInfo(string baseUrl, HttpMessageHandler clientHandler = null)
        {
            if (!baseUrl.EndsWith("/")) baseUrl += '/';
            BaseUri = new Uri(baseUrl);

            if (clientHandler == null) clientHandler = new HttpClientHandler();
            if (clientHandler is HttpClientHandler hdl)
            {
                hdl.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
#if !NETSTANDARD
            if (clientHandler is SocketsHttpHandler shdl)
            {
                shdl.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
#endif

            httpClient = new HttpClient(clientHandler);
            if (GlobalDefaultTimeout != null) httpClient.Timeout = GlobalDefaultTimeout.Value;

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
        /// Set Authorization header with as Basic authentication
        /// </summary>
        public void SetAuthorizationBasic(string user, string password)
        {
            var str = $"{user}:{password}";

#if NETSTANDARD1_1
            var bytes = str.ToCharArray().Select(o => (byte)o).ToArray();
#else
            var bytes = Encoding.ASCII.GetBytes(str);
#endif

            var b64 = Convert.ToBase64String(bytes);
            SetAuthorization("Basic " + b64);
        }

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

            return await sendMessageAsync<T>(uri, msg);
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

            return await sendMessageAsync(msg);
        }
        /// <summary>
        /// Sends a Delete request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        public async Task<Response<T>> DeleteAsync<T>(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Delete, uri);

            return await sendMessageAsync<T>(uri, msg);
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

            using var msg = new HttpRequestMessage(HttpMethod.Post, uri);
            msg.Content = buildJsonContent(value, msg);

            return await sendMessageAsync<T>(uri, msg);
        }
        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        public async Task<Response> PostAsync(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Post, uri);

            return await sendMessageAsync(msg);
        }
        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PostAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Post, uri);
            msg.Content = buildJsonContent(value, msg);
            return await sendMessageAsync(msg);
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
            using var msg = new HttpRequestMessage(HttpMethod.Post, uri);
            if (content is not null) msg.Content = content;

            return await sendMessageAsync<T>(uri, msg);
        }

        /* PUT */
        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response<T>> PutAsync<T>(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Put, uri);
            msg.Content = buildJsonContent(value, msg);

            return await sendMessageAsync<T>(uri, msg);
        }
        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PutAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var msg = new HttpRequestMessage(HttpMethod.Put, uri);
            msg.Content = buildJsonContent(value, msg);

            return await sendMessageAsync(msg);
        }

        /* PATCH */
        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response<T>> PatchAsync<T>(string service, object value)
        {
            var uri = new Uri(BaseUri, service);

            using var msg = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            msg.Content = buildJsonContent(value, msg);

            return await sendMessageAsync<T>(uri, msg);
        }
        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PatchAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);

            using var msg = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            msg.Content = buildJsonContent(value, msg);

            return await sendMessageAsync(msg);
        }

        /* OPTIONS */
        /// <summary>
        /// Sends an Options request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Object with fields to be mapped</param>
        public async Task<Response> OptionsAsync(string service, object value)
            => await OptionsAsync(service, Helper.buildParams(value, NullParameterHandlingPolicy_IgnoreNulls));

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

            return await sendMessageAsync(message);
        }

        private async Task<Response> sendMessageAsync(HttpRequestMessage message)
        {
            BeforeSend?.Invoke(this, message);
            DateTime start = DateTime.Now; // not include BeforeSend execution
            using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            return Response.Build(response, start);
        }
        private async Task<Response<T>> sendMessageAsync<T>(Uri uri, HttpRequestMessage message)
        {
            BeforeSend?.Invoke(this, message);
            DateTime start = DateTime.Now; // not include BeforeSend execution

            if (typeof(T) == typeof(System.IO.Stream))
            {
                // WITHOUT USING
                var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead);
                return await processResponseAsync<T>(uri, response, start);
            }
            else
            {
                using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
                return await processResponseAsync<T>(uri, response, start);
            }
        }

        private HttpContent buildJsonContent(object value, HttpRequestMessage msg)
        {
            var jsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(value, new Newtonsoft.Json.JsonSerializerSettings()
            {
                NullValueHandling = NullParameterHandlingPolicy_IgnoreNulls ? Newtonsoft.Json.NullValueHandling.Ignore : Newtonsoft.Json.NullValueHandling.Include,
            });

            if (JsonSerializeOverride != null)
            {
                var args = new JsonSerializeArgs
                {
                    Request = msg,
                    Handled = false,
                    Object = value,
                    Value = jsonValue,
                };
                JsonSerializeOverride(this, args);
                if (args.Handled)
                {
                    jsonValue = args.Value;
                }
            }

            return new StringContent(jsonValue, Encoding.UTF8, "application/json");
        }
        private async Task<Response<T>> processResponseAsync<T>(Uri uri, HttpResponseMessage response, DateTime start)
        {
            T data = default;

            bool binary = false;

            string content = null;
            if (typeof(T) == typeof(byte[]))
            {
                data = (T)(object)await response.Content.ReadAsByteArrayAsync();
                binary = true;
            }
            else if (typeof(T) == typeof(System.IO.Stream))
            {
                data = (T)(object)await response.Content.ReadAsStreamAsync();
                binary = true;
            }
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
                else if (typeof(T) == typeof(JObject))
                {
                    data = (T)(object)JObject.Parse(content);
                }
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
                else
                {
                    if (content.StartsWith("<")) // xml
                    {
#if NETSTANDARD1_1
                        throw new NotSupportedException("NET 1.1 do not support XML serialization");
#else
                        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                        using var reader = new System.IO.StringReader(content);
                        data = (T)serializer.Deserialize(reader);
#endif
                    }
                    else if (DeserializeJValueOverride == null && DeserializeJObjectOverride == null)
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
                    }
                    else
                    {
                        var obj = JObject.Parse(content);
                        // Entire Object
                        if (DeserializeJObjectOverride != null)
                        {
                            var args = new DeserializeJObjectOverrideArgs
                            {
                                Response = response,
                                TargetType = typeof(T),
                                JsonContent = content,
                                Value = obj,
                            };
                            DeserializeJObjectOverride.Invoke(this, args);
                        }
                        // Values
                        if (DeserializeJValueOverride != null)
                        {
                            foreach (var value in obj.Descendants().OfType<JValue>())
                            {
                                var args = new DeserializeJValueOverrideArgs
                                {
                                    Handled = false,
                                    Value = new JValue(value), //value.DeepClone(),
                                };
                                DeserializeJValueOverride.Invoke(this, args);

                                if (args.Handled)
                                {
                                    value.Replace(args.Value);
                                }
                            }
                        }

                        data = obj.ToObject<T>();
                    }
                }
            }

            var d = Response<T>.Build(response, contentHeaders, data, errorData, start);

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
        public class DeserializeJValueOverrideArgs
        {
            public bool Handled { get; set; }
            public JValue Value { get; set; }
        }
        public class JsonSerializeArgs
        {
            public HttpRequestMessage Request { get; set; }
            public object Object { get; set; }
            public string? Value { get; set; }
            public bool Handled { get; set; }
        }
        public class DeserializeJObjectOverrideArgs
        {
            public HttpResponseMessage Response { get; set; }
            public Type TargetType { get; set; }
            public string JsonContent { get; set; }
            public JObject Value { get; set; }
        }
    }
}
