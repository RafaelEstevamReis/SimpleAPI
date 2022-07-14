using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.API
{
    /// <summary>
    /// Entensions for the client
    /// </summary>
    public static class ClientInfoExtensions
    {
        /* GET */
        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response<T>> GetAsync<T>(this ClientInfo client, string service, int id)
        {
            return await client.GetAsync<T>($"{service}/{id}");
        }
        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response<T>> GetAsync<T>(this ClientInfo client, string service, Guid id)
        {
            return await client.GetAsync<T>($"{service}/{id}");
        }

        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="values">Url get parameters</param>
        public static async Task<Response<T>> GetAsync<T>(this ClientInfo client, string service, KeyValuePair<string, string>[] values)
        {
            string url = Helper.buildUrl(service, values);
            return await client.GetAsync<T>(url);
        }

        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="p">Builds url get parameters</param>
        public static async Task<Response<T>> GetAsync<T>(this ClientInfo client, string service, object p)
        {
            string url = Helper.buildUrl(service, Helper.buildParams(p));
            return await client.GetAsync<T>(url);
        }

        /* DELETE */

        /// <summary>
        /// Sends a Delete request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> DeleteAsync(this ClientInfo client, string service, int id)
        {
            return await client.DeleteAsync($"{service}/{id}");
        }
        /// <summary>
        /// Sends a Delete request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> DeleteAsync(this ClientInfo client, string service, Guid id)
        {
            return await client.DeleteAsync($"{service}/{id}");
        }

        /* POST */

        /// <summary>
        /// Sends a Post request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response<T>> PostAsync<T>(this ClientInfo client, string service, object value, int id)
        {
            return await client.PostAsync<T>($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Post request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response<T>> PostAsync<T>(this ClientInfo client, string service, object value, Guid id)
        {
            return await client.PostAsync<T>($"{service}/{id}", value);
        }

        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> PostAsync(this ClientInfo client, string service, object value, int id)
        {
            return await client.PostAsync($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> PostAsync(this ClientInfo client, string service, object value, Guid id)
        {
            return await client.PostAsync($"{service}/{id}", value);
        }

        /// <summary>
        /// Sends a Post request with Multipart Form Data content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public static async Task<Response<T>> MultipartFormPostAsync<T>(this ClientInfo client, string service, Dictionary<string, string> fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var pair in fields) lst.Add(new KeyValuePair<string, string>(pair.Key, pair.Value));
            return await MultipartFormPostAsync<T>(client, service, lst);
        }
        /// <summary>
        /// Sends a Post request with Multipart Form Data content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public static async Task<Response<T>> MultipartFormPostAsync<T>(this ClientInfo client, string service, NameValueCollection fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var k in fields.AllKeys) lst.Add(new KeyValuePair<string, string>(k, fields[k]));
            return await MultipartFormPostAsync<T>(client, service, lst);
        }
        /// <summary>
        /// Sends a Post request with Multipart Form Data content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public static async Task<Response<T>> MultipartFormPostAsync<T>(this ClientInfo client, string service, IEnumerable<KeyValuePair<string, string>> fields)
        {
            var formContent = new MultipartFormDataContent();
            foreach (var item in fields)
            {
                formContent.Add(new StringContent(item.Value), item.Key);
            }

            return await client.PostAsync<T>(service, formContent);
        }

        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public static async Task<Response<T>> FormUrlEncodedPostAsync<T>(this ClientInfo client, string service, Dictionary<string, string> fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var pair in fields) lst.Add(new KeyValuePair<string, string>(pair.Key, pair.Value));
            return await FormUrlEncodedPostAsync<T>(client, service, lst);
        }
        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public static async Task<Response<T>> FormUrlEncodedPostAsync<T>(this ClientInfo client, string service, NameValueCollection fields)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            foreach (var k in fields.AllKeys) lst.Add(new KeyValuePair<string, string>(k, fields[k]));
            return await FormUrlEncodedPostAsync<T>(client, service, lst);
        }
        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="fields">Form values</param>
        public static async Task<Response<T>> FormUrlEncodedPostAsync<T>(this ClientInfo client, string service, IEnumerable<KeyValuePair<string, string>> fields)
        {
            var content = new FormUrlEncodedContent(fields);
            return await client.PostAsync<T>(service, content);
        }
        /// <summary>
        /// Sends a Post request with Form Url Encoded content and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="values">Object with fields to be mapped</param>
        public static async Task<Response<T>> FormUrlEncodedPostAsync<T>(this ClientInfo client, string service, object values)
            => await FormUrlEncodedPostAsync<T>(client, service, Helper.buildParams(values));

        /* PUT */

        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> PutAsync(this ClientInfo client, string service, object value, int id)
        {
            return await client.PutAsync($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> PutAsync(this ClientInfo client, string service, object value, Guid id)
        {
            return await client.PutAsync($"{service}/{id}", value);
        }

        /* PATCH */

        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> PatchAsync(this ClientInfo client, string service, object value, int id)
        {
            return await client.PatchAsync($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task<Response> PatchAsync(this ClientInfo client, string service, object value, Guid id)
        {
            return await client.PatchAsync($"{service}/{id}", value);
        }
    }
}
