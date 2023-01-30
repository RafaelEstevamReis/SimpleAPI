using System.Net.Http;

namespace Simple.API
{
    public class UnsuccessfulStatusCodeException : HttpRequestException
    {
        public UnsuccessfulStatusCodeException(Response response)
            : base($"[{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} [{(int)response.StatusCode}] failed with {response.ReasonPhrase}")
        {
            Response = response;
        }

        public Response Response { get; }
    }
    public class UnsuccessfulStatusCodeException<T> : HttpRequestException
    {
        public UnsuccessfulStatusCodeException(Response response, T error)
            : base($"[{response.RequestMessage.Method}] {response.RequestMessage.RequestUri} [{(int)response.StatusCode}] failed with {response.ReasonPhrase}")
        {
            Response = response;
            ErrorInformation = error;
        }

        public Response Response { get; }
        public T ErrorInformation { get; }
    }
}
