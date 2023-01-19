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
}
