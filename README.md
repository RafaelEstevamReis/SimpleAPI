# SimpleAPI
[![.NET](https://github.com/RafaelEstevamReis/SimpleAPI/actions/workflows/dotnet.yml/badge.svg)](https://github.com/RafaelEstevamReis/SimpleAPI)
[![NuGet](https://buildstats.info/nuget/Simple.API)](https://www.nuget.org/packages/Simple.API)

A simple C# REST API client implementation

# Table of Contents
<!-- TOC -->
- [SimpleAPI](#simpleapi)
- [Table of Contents](#table-of-contents)
  - [Compatibility](#compatibility)
  - [Installing](#installing)
  - [Basic Use](#basic-use)
  - [API Builder](#api-builder)
<!-- /TOC -->

## Compatibility

Huge compatibility, currently supports:
* .Net 8
* .Net 6
* .Net Core 3.1
* .Net Framework 4.5
* .Net Standard 1.1
  * .NetCore 1.0+
  * .Net Framework 4.5
  * Mono 4.6+
  * Xamarin.iOS 10.0+
  * Xamarin.Android 7.0+
  * UWP 8.0+
  * Unity 2018.1+

## Installing

Get from NuGet and start testing

> PM> [Install-Package Simple.API](https://www.nuget.org/packages/Simple.API)

## Basic Use

How to GET a resource at `https://httpbin.org/anything/42`
~~~C#
// Base address is used to reuse the client object 
// but also do not have to repeat all the address
var client = new ClientInfo("https://httpbin.org/");

/* GET */
// no params
var getResponse = await client.GetAsync<TestResponse>("anything");
// object builded parameter
var t = await client.GetAsync<TestResponse>("anything", new { id = "1234", value = 12.34 });
/* POST */
var postResponse = await client.PostAsync<TestResponse>("anything", new { id = "1234", value = 12.34 });
/* And all others */
var putResponse = await client.PutAsync("anything", data);
var patchResponse = await client.PatchAsync("anything", data);
var deleteResponse = await client.DeleteAsync("anything");
~~~

The response object contains the original data, status code, request and response headers

~~~C#
if(!putResponse.IsSuccessStatusCode) 
  Console.WriteLine("☹️");
if(patchResponse.StatusCode == HttpStatusCode.Forbidden) 
  Console.WriteLine("🔒");

// Validate response, throws if necessary and gets data
var data = getResponse.GetSuccessfulData();
~~~

Authentication:
~~~C#
// Using with JWT Token:
client.SetAuthorizationBearer(jwtToken);
// Using with BasicAuth
client.SetAuthorizationBasic(user, password);

// Using with other options (manually set)
client.SetAuthorization(headerKey, value);

// For other headers (like x-auth)
client.SetHeader("x-auth", xToken);
~~~

JWT parsing:
~~~C#
// Parse JWT
var jwt = JWT.Parse(token);
// Access commom fields
bool valid = jwt.Content.GetExp > DateTime.Now;

// Or parse your custom Model
var customJwt = JWT<YourModel>.Parse(token);
~~~


## API Builder

This library supports implicit implementation, just create an interface for the API and the builder takes care of the implementation:

~~~ C#
public interface IMyAPI
{
    /* Standard Responses with response metadata */
    [Get("anything")]
    Task<Response<TestResponse>> GetAnythingAsync();

    [Post("anything")]
    Task<Response<TestResponse>> PostNothingAsync();

    /* Validated with GetSuccessfulData */
    [Get("anything")]
    Task<TestResponse> GetAnythingSuccessfulAsync();

    [Post("anything")]
    Task<TestResponse> PostAnythingSuccessfulAsync(TestData d);

    /* Internal settings and interactions */
    ClientInfo GetInternalClient();
    void SetAuthorizationBearer(string jwt);
}
~~~

Then you will be able to just:

~~~ C#
var myClient = ClientBuilder.Create<IMyAPI>("https://httpbin.org/");
myClient.SetAuthorizationBearer("Your JWT here");

// Standard GET
var resultGet = await myClient.GetAnythingAsync();
// Post with validated result
var resultPost = await myClient.PostAnythingSuccessfulAsync(new { a = 1 });
~~~

