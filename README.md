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
  - [Use](#use)
<!-- /TOC -->

## Compatibility

Huge compatibility, currently supports:
* .Net 5
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

## Use

How to GET a resource at `https://httpbin.org/anything/42`
~~~C#
// Base address is used to reuse the client object 
// but also do not have to repeat all the address
API.Client client = new API.Client("https://httpbin.org/");

var getInt = await client.GetAsync<TestResponse>("anything", 42);
var getParam = await client.GetAsync<TestResponse>("anything", new { id = 42 });
~~~


