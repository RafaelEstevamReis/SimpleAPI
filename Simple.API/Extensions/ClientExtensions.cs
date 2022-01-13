using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.API
{
    /// <summary>
    /// Entensions for the client
    /// </summary>
    public static class ClientExtensions
    {
        /* GET */

        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="id">Service/action id</param>
        public static async Task<T> GetAsync<T>(this Client client, string service, int id)
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
        public static async Task<T> GetAsync<T>(this Client client, string service, Guid id)
        {
            return await client.GetAsync<T>($"{service}/{id}");
        }

        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="values">Url get parameters</param>
        public static async Task<T> GetAsync<T>(this Client client, string service, KeyValuePair<string, string>[] values)
        {
            string url = Helper.buildUrl(service, values);
            return await client.GetAsync<T>(url);
        }
        /// <summary>
        /// Sends a Get request and process the returned content
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="p">Builds url get parameters</param>
        public static async Task<T> GetAsync<T>(this Client client, string service, object p)
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
        public static async Task DeleteAsync(this Client client, string service, int id)
        {
            await client.DeleteAsync($"{service}/{id}");
        }
        /// <summary>
        /// Sends a Delete request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="id">Service/action id</param>
        public static async Task DeleteAsync(this Client client, string service, Guid id)
        {
            await client.DeleteAsync($"{service}/{id}");
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
        public static async Task<T> PostAsync<T>(this Client client, string service, object value, int id)
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
        public static async Task<T> PostAsync<T>(this Client client, string service, object value, Guid id)
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
        public static async Task PostAsync(this Client client, string service, object value, int id)
        {
            await client.PostAsync($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Post request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task PostAsync(this Client client, string service, object value, Guid id)
        {
            await client.PostAsync($"{service}/{id}", value);
        }

        /* PUT */

        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task PutAsync(this Client client, string service, object value, int id)
        {
            await client.PutAsync($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Put request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task PutAsync(this Client client, string service, object value, Guid id)
        {
            await client.PutAsync($"{service}/{id}", value);
        }

        /* PATCH */

        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task PatchAsync(this Client client, string service, object value, int id)
        {
            await client.PatchAsync($"{service}/{id}", value);
        }
        /// <summary>
        /// Sends a Patch request
        /// </summary>
        /// <param name="client">API client to use</param>
        /// <param name="service">Service to request from, will be concatenated with BaseUri</param>
        /// <param name="value">Value to be sent</param>
        /// <param name="id">Service/action id</param>
        public static async Task PatchAsync(this Client client, string service, object value, Guid id)
        {
            await client.PatchAsync($"{service}/{id}", value);
        }

    }
}
