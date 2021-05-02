using System;
using System.Net;
using System.Net.Http;

namespace Simple.API
{
    public class ApiException : Exception
    {
        public Uri Resource { get; }
        public HttpStatusCode StatusCode { get; }
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
