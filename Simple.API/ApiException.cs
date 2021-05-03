using System;
using System.Net;
using System.Net.Http;

namespace Simple.API
{
    /// <summary>
    /// Represents an erro StatusCode
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// Resource used
        /// </summary>
        public Uri Resource { get; }
        /// <summary>
        /// Status code returned
        /// </summary>
        public HttpStatusCode StatusCode { get; }
        /// <summary>
        /// Response returned
        /// </summary>
        public HttpResponseMessage Response { get; }

        //public ApiException() { }
        //public ApiException(string message) : base(message) { }
        //public ApiException(string message, Exception innerException) : base(message, innerException) { }

        private ApiException(Uri resource, HttpResponseMessage response)
            : base($"[{(int)response.StatusCode}] {resource} - {response.ReasonPhrase}")
        {
            StatusCode = response.StatusCode;
            Resource = resource;
            Response = response;
        }

        internal static Exception FromResponse(Uri resource, HttpResponseMessage response)
        {
            return new ApiException(resource, response);
        }
    }
}
