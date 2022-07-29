using System;
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
        /// Gets the collection of HTTP content headers
        /// </summary>
        public HttpContentHeaders ContentHeaders { get; protected set; }
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
        /// Gets string response on Non-SuccessStatusCode
        /// </summary>
        public string ErrorResponseData { get; protected set; }
        /// <summary>
        /// Request duration
        /// </summary>
        public TimeSpan Duration { get; protected set; }

        /// <summary>
        /// Parses json ErrorResponseData as `T`
        /// </summary>
        public T ParseErrorResponseData<T>()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(ErrorResponseData);
        }
        /// <summary>
        /// Throws an HttpRequestException if not IsSuccessStatusCode
        /// </summary>
        /// <exception cref="HttpRequestException">HttpRequestException if not IsSuccessStatusCode</exception>
        public void EnsureSuccessStatusCode()
        {
            if (IsSuccessStatusCode) return;

            throw new HttpRequestException($"[{RequestMessage.Method}] {RequestMessage.RequestUri} [{(int)StatusCode}] failed with {ReasonPhrase}");
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public static Response Build(HttpResponseMessage response, DateTime start)
        {
            string errorData = null;
            if (!response.IsSuccessStatusCode) errorData = response.Content.ReadAsStringAsync().Result;

            return new Response()
            {
                Headers = response.Headers,
                ContentHeaders = response.Content.Headers,
                RequestMessage = response.RequestMessage,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode,
                ErrorResponseData = errorData,
                Duration = DateTime.Now - start,
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
        public static Response<T> Build(HttpResponseMessage response, HttpContentHeaders headers, T data, string errorData, DateTime start)
        {
            return new Response<T>()
            {
                Data = data,
                Headers = response.Headers,
                ContentHeaders = headers,
                RequestMessage = response.RequestMessage,
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase,
                StatusCode = response.StatusCode,
                ErrorResponseData = errorData,
                Duration = DateTime.Now - start,
            };
        }
    }
}
