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

        ;
        Console.WriteLine("End");
    }

    public interface IDemo
    {
        /* Normal Respnse return */
        [Get("anything")]
        Task<Response<TestResponse>> GetAnythingAsync();

        [Post("anything")]
        Task<Response<TestResponse>> PostNothingAsync();

        [Post("anything")]
        Task<Response<TestResponse>> PostAnythingAsync(TestData d);

        [Put("anything")]
        Task<Response<TestResponse>> PutAnythingAsync(TestData d);

        /* Validate with GetSuccessfulData */
        [Post("anything")]
        Task<TestResponse> PostAnythingSuccessfulAsync(TestData d);
    }

}
