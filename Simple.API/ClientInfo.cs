using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private readonly HttpClient httpClient;
        /// <summary>
        /// Base url of the API
        /// </summary>
        public readonly Uri BaseUri;
        /// <summary>
        /// Creates a new insance
        /// </summary>
        /// <param name="baseUrl">Base url of the API</param>
        public ClientInfo(string baseUrl)
        {
            if (!baseUrl.EndsWith("/")) baseUrl += '/';
             BaseUri = new Uri(baseUrl);
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        }
        /// <summary>
        /// Configures the underlying HttpClient
        /// </summary>
        public void ConfigureHttpClient(Action<HttpClient> client) => client(httpClient);

        /* GET */
        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        public async Task<Response<T>> GetAsync<T>(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.GetAsync(uri);
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
            using var response = await httpClient.DeleteAsync(uri);
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
        public async Task<Response<T>> FormUrlEncodedPostAsync<T>(string service, Dictionary<string,string> fields)
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

        /* PUT */
        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<Response> PutAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PutAsync(uri, buildContent(value));
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
            using var response = await httpClient.PatchAsync(uri, buildContent(value));
            return Response.Build(response);
        }

        private HttpContent buildContent(object value)
        {
            var jsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return new StringContent(jsonValue, Encoding.UTF8, "application/json");
        }
        private static async Task<Response<T>> processResponseAsync<T>(Uri uri, HttpResponseMessage response)
        {
            T data = default;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (typeof(T) == typeof(string)) data = (T)(object)json;
                else data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }

            return Response<T>.Build(response, data);
        }
    }
}
