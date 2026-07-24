namespace Simple.API;

using System.Net.Http;

/// <summary>
/// Thrown when a request completes with a non-success status code
/// </summary>
/// <remarks>
/// Creates a new instance from the failed response
/// </remarks>
/// <param name="response">The response that carried the unsuccessful status code</param>
public class UnsuccessfulStatusCodeException(Response response) : HttpRequestException($"[{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} [{(int)response.StatusCode}] failed with {response.ReasonPhrase}")
{

    /// <summary>
    /// The response that carried the unsuccessful status code
    /// </summary>
    public Response Response { get; } = response;
}

/// <summary>
/// Thrown when a request completes with a non-success status code, carrying the parsed error body
/// </summary>
/// <typeparam name="T">Type the error response body is parsed into</typeparam>
/// <remarks>
/// Creates a new instance from the failed response and parsed error body
/// </remarks>
/// <param name="response">The response that carried the unsuccessful status code</param>
/// <param name="error">The error response body parsed as <typeparamref name="T"/></param>
public class UnsuccessfulStatusCodeException<T>(Response response, T error) : HttpRequestException($"[{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} [{(int)response.StatusCode}] failed with {response.ReasonPhrase}")
{

    /// <summary>
    /// The response that carried the unsuccessful status code
    /// </summary>
    public Response Response { get; } = response;
    /// <summary>
    /// The error response body parsed as <typeparamref name="T"/>
    /// </summary>
    public T ErrorInformation { get; } = error;
}
