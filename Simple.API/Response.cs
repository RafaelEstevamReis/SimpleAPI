using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Simple.API
{
    /// <summary>
    /// API response
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Gets the collection of HTTP response headers
        /// </summary>
        public HttpResponseHeaders Headers { get; protected set; }
        /// <summary>
        /// Gets or sets the request message which led to this response message
        /// </summary>
        public HttpRequestMessage RequestMessage { get; protected set; }
        /// <summary>
        /// Gets a value that indicates if the HTTP response was successful
        /// </summary>
        public bool IsSuccessStatusCode { get; protected set; }
        /// <summary>
        /// Gets or sets the reason phrase which typically is sent by servers together with
        /// the status code
        /// </summary>
        public string ReasonPhrase { get; protected set; }
        /// <summary>
        /// Gets or sets the status code of the HTTP response
        /// </summary>
        public HttpStatusCode StatusCode { get; protected set; }

        /// <summary>
        /// Create a new isntance
        /// </summary>
        public static Response Build(HttpResponseMessage response)
        {
            return new Response()
            {
                Headers = response.Headers,
                RequestMessage = response.RequestMessage,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode
            };
        }
    }
    /// <summary>
    /// API Response with data
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    public class Response<T> : Response
    {
        /// <summary>
        /// Deserialized data
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Create a new isntance
        /// </summary>
        public static Response<T> Build(HttpResponseMessage response, T data)
        {
            return new Response<T>()
            {
                Data = data,
                Headers = response.Headers,
                RequestMessage = response.RequestMessage,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode
            };
        }
    }
}
