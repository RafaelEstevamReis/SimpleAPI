using Simple.API;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Test
{
    class Example_ClientInfo
    {
        public static async Task Run()
        {
            Console.WriteLine("API Test!");

            Guid guid = Guid.NewGuid();
            TestData data = new TestData()
            {
                Id = 1,
                Uid = Guid.NewGuid(),
                Text = "This is a test",
                //Number = 42,
                DoubleFloatPoint = 3.14
            };

            var client = new ClientInfo("https://httpbin.org/");
            // Request Signing example
            // client.JsonSerializeOverride += (object sender, ClientInfo.JsonSerializeArgs e) =>
            // {
            //     // Sign
            //     string signatureBase = $"{e.Request.Method}{e.Request.RequestUri}{e.Value}";
            //     string signed = Convert.ToBase64String(Encoding.UTF8.GetBytes(signatureBase)); // Simulates request signing
            //     e.Request.Headers.Add("X-SIGN", signed);
            // };

            /* GET */
            // no params
            var get = await client.GetAsync<TestResponse>("anything");
            // int param
            var getInt = await client.GetAsync<TestResponse>("anything", 42);
            // guid param
            var getGuid = await client.GetAsync<TestResponse>("anything", guid);
            // object builded param
            var t = await client.GetAsync<TestResponse>("anything", new { id = "1234", value = 12.34 });
            // object param
            var o = await client.GetAsync<TestResponse>("anything", data);

            /* DELETE */
            // no params
            await client.DeleteAsync("anything");
            // int param
            await client.DeleteAsync("anything", 42);
            // guid param
            await client.DeleteAsync("anything", guid);

            /* POST - With response */
            // no params
            var post = await client.PostAsync<TestResponse>("anything", data);
            // int param
            var postInt = await client.PostAsync<TestResponse>("anything", data, 42);
            // guid param
            var postGuid = await client.PostAsync<TestResponse>("anything", data, guid);

            /* POST - No response */
            // no params
            await client.PostAsync("anything", data);
            // int param
            await client.PostAsync("anything", data, 42);
            // guid param
            await client.PostAsync("anything", data, guid);

            /* PUT */
            // no params
            await client.PutAsync("anything", data);
            // int param
            await client.PutAsync("anything", data, 42);
            // guid param
            await client.PutAsync("anything", data, guid);

            /* PATCH */
            // no params
            await client.PatchAsync("anything", data);
            // int param
            await client.PatchAsync("anything", data, 42);
            // guid param
            await client.PatchAsync("anything", data, guid);

            Console.WriteLine("End");
        }

    }
}
