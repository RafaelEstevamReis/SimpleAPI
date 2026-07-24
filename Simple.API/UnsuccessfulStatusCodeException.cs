using System.Net.Http;

namespace Simple.API
{
    /// <summary>
    /// Thrown when a request completes with a non-success status code
    /// </summary>
    public class UnsuccessfulStatusCodeException : HttpRequestException
    {
        /// <summary>
        /// Creates a new instance from the failed response
        /// </summary>
        /// <param name="response">The response that carried the unsuccessful status code</param>
        public UnsuccessfulStatusCodeException(Response response)
            : base($"[{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} [{(int)response.StatusCode}] failed with {response.ReasonPhrase}")
        {
            Response = response;
        }

        /// <summary>
        /// The response that carried the unsuccessful status code
        /// </summary>
        public Response Response { get; }
    }
    /// <summary>
    /// Thrown when a request completes with a non-success status code, carrying the parsed error body
    /// </summary>
    /// <typeparam name="T">Type the error response body is parsed into</typeparam>
    public class UnsuccessfulStatusCodeException<T> : HttpRequestException
    {
        /// <summary>
        /// Creates a new instance from the failed response and parsed error body
        /// </summary>
        /// <param name="response">The response that carried the unsuccessful status code</param>
        /// <param name="error">The error response body parsed as <typeparamref name="T"/></param>
        public UnsuccessfulStatusCodeException(Response response, T error)
            : base($"[{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} [{(int)response.StatusCode}] failed with {response.ReasonPhrase}")
        {
            Response = response;
            ErrorInformation = error;
        }

        /// <summary>
        /// The response that carried the unsuccessful status code
        /// </summary>
        public Response Response { get; }
        /// <summary>
        /// The error response body parsed as <typeparamref name="T"/>
        /// </summary>
        public T ErrorInformation { get; }
    }
}
