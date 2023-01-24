using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Simple.API
{
    /// <summary>
    /// Simple json-based api client with more information
    /// </summary>
    [Obsolete("Use ClientInfo instead", true)]
    public class Client
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
        public Client(string baseUrl)
        {
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
        public async Task<T> GetAsync<T>(string service)
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
        public async Task DeleteAsync(string service)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.DeleteAsync(uri);
            processResponse(uri, response);
        }

        /* POST */
        /// <summary>
        /// Sends a Post request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task<T> PostAsync<T>(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, buildContent(value));
            return await processResponseAsync<T>(uri, response);
        }

        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task PostAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PostAsync(uri, buildContent(value));
            processResponse(uri, response);
        }

        /// <summary>
        /// Sends a Post request with specified content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="content">Content to be sent</param>
        public async Task<T> PostAsync<T>(string service, HttpContent content)
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
        /// <param name="values">Form values</param>
        public async Task<T> MultipartFormPostAsync<T>(string service, KeyValuePair<string, string>[] values)
        {
            var formContent = new MultipartFormDataContent();
            foreach (var item in values)
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
        /// <param name="values">Form values</param>
        public async Task<T> FormUrlEncodedPostAsync<T>(string service, KeyValuePair<string, string>[] values)
        {
            var formContent = new FormUrlEncodedContent(values);

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
        public async Task PutAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PutAsync(uri, buildContent(value));
            processResponse(uri, response);
        }

        /* PATCH */
        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        public async Task PatchAsync(string service, object value)
        {
            var uri = new Uri(BaseUri, service);
            using var response = await httpClient.PatchAsync(uri, buildContent(value));
            processResponse(uri, response);
        }

        private HttpContent buildContent(object value)
        {
            var jsonValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return new StringContent(jsonValue, Encoding.UTF8, "application/json");
        }
        private static async Task<T> processResponseAsync<T>(Uri uri, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (typeof(T) == typeof(string)) return (T)(object)json;

                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                throw ApiException.FromResponse(uri, response);
            }
        }
        private static void processResponse(Uri uri, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) throw ApiException.FromResponse(uri, response);
        }
    }
}
