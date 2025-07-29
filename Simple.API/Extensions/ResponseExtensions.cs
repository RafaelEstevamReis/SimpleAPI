namespace Simple.API;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static class ResponseExtensions
{
    public static T GetSuccessfulData<T>(this Response<T> response)
    {
        response.EnsureSuccessStatusCode();
        return response.Data;
    }
    public static async Task<T> GetSuccessfulData<T>(this Task<Response<T>> responseTask)
    {
        return (await responseTask).GetSuccessfulData();
    }

#if !NETSTANDARD1_1 && !NETSTANDARD2_0
    public static async Task SaveSuccessfulData(this Task<Response<byte[]>> responseTask, string filePath)
    {
        var bytes = (await responseTask).GetSuccessfulData();

        var fi = new FileInfo(filePath);
        if (!fi.Directory.Exists) fi.Directory.Create();

        await File.WriteAllBytesAsync(filePath, bytes);
    }
    public static async Task SaveSuccessfulData(this Task<Response<Stream>> responseTask, string filePath)
    {
        using var dataStream = (await responseTask).GetSuccessfulData(); // Throwns exception before touches filesystem
        var fi = new FileInfo(filePath);
        if (!fi.Directory.Exists) fi.Directory.Create();

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        dataStream.CopyTo(fs);
    }

    public static async Task ReadSSE(this Response<Stream> response, Action<string> textEvents, CancellationToken token)
    {
        using var reader = new StreamReader(response.Data);
        while (!reader.EndOfStream && !token.IsCancellationRequested)
        {
            string line = await reader.ReadLineAsync();
            textEvents(line);
        }
    }
    public static async Task ReadSSE(this Task<Response<Stream>> responseTask, Action<string> textEvents, CancellationToken token)
        => await (await responseTask).ReadSSE(textEvents: textEvents, token);

    public static async Task ReadSSE(this Response<Stream> response, Action<Newtonsoft.Json.Linq.JObject> jEvents, CancellationToken token)
    {
        await response.ReadSSE(textEvents: (l) => jEvents(Newtonsoft.Json.Linq.JObject.Parse(l)), token);
    }
    public static async Task ReadSSE(this Task<Response<Stream>> responseTask, Action<Newtonsoft.Json.Linq.JObject> jEvents, CancellationToken token)
    {
        await (await responseTask).ReadSSE(jEvents: jEvents, token);
    }
#endif
}
