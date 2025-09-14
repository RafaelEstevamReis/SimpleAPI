namespace Simple.Test;

using Simple.API;
using Simple.API.ClientBuilderAttributes;
using System;
using System.Threading.Tasks;

public class Example_ClientBuilder
{
    internal static async Task Run()
    {
        var demoClient = ClientBuilder.Create<IDemo>("https://httpbin.org/");

        var resultGet = await demoClient.GetAnythingAsync();
        var resultNPost = await demoClient.PostNothingAsync();
        var resultPost = await demoClient.PostAnythingAsync(new TestData
        {
            Number = 42
        });
        var resultPut = await demoClient.PutAnythingAsync(new TestData
        {
            Number = 42
        });

        // Using GetSuccessfulData on return
        var resultPostD = await demoClient.PostAnythingSuccessfulAsync(new TestData
        {
            Number = 42
        });

        // Accessing internal client
        var client = demoClient.GetInternalClient();
        // Setting authorization
        demoClient.SetAuthorizationBearer("Your JWT here");
        demoClient.SetHeader("Api-Key", "MyApiKey");

        ;
        Console.WriteLine("End");
    }

    [Timeout(timeoutInSeconds: 20)] // Optional Attribute for setting Client's Timeout
    public interface IDemo
    {
        /* Normal Responses */
        [Get("anything")]
        Task<Response<TestResponse>> GetAnythingAsync();

        [Post("anything")]
        Task<Response<TestResponse>> PostNothingAsync();

        [Post("anything")]
        Task<Response<TestResponse>> PostAnythingAsync(TestData d);

        [Put("anything")]
        Task<Response<TestResponse>> PutAnythingAsync(TestData d);

        /* Validated with GetSuccessfulData */
        [Post("anything")]
        Task<TestResponse> PostAnythingSuccessfulAsync(TestData d);

        /* Internal settings */
        ClientInfo GetInternalClient();
        void SetAuthorizationBearer(string jwt);
        void SetHeader(string key, string value);
    }

}
