using System;
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
