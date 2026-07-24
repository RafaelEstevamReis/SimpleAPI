namespace Simple.API;

using System;
using System.IO;
using System.Text;
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
        response.EnsureSuccessStatusCode();
        using var reader = new StreamReader(response.Data, Encoding.UTF8);

        var data = new StringBuilder();
        bool hasData = false;

        while (!token.IsCancellationRequested)
        {
#if NET7_0_OR_GREATER
            string line = await reader.ReadLineAsync(token);
#else
            string line = await reader.ReadLineAsync();
#endif
            if (line == null) break; // end of stream

            if (line.Length == 0)
            {
                // Blank line dispatches the buffered event.
                // Per spec, an event not terminated by a blank line (incomplete at
                // end of stream) is discarded, so no flush after the loop.
                if (hasData)
                {
                    textEvents(data.ToString());
                    data.Clear();
                    hasData = false;
                }
                continue;
            }
            if (line[0] == ':') continue; // comment line

            // field parsing: "field" or "field:value" with one optional leading space
            int colon = line.IndexOf(':');
            string field = colon < 0 ? line : line.Substring(0, colon);
            string value = colon < 0 ? string.Empty : line.Substring(colon + 1);
            if (value.Length > 0 && value[0] == ' ') value = value.Substring(1);

            // event/id/retry are ignored by the data-only API
            if (field == "data")
            {
                if (hasData) data.Append('\n');
                data.Append(value);
                hasData = true;
            }
        }
    }
    public static async Task ReadSSE(this Task<Response<Stream>> responseTask, Action<string> textEvents, CancellationToken token)
        => await (await responseTask).ReadSSE(textEvents: textEvents, token);

    public static async Task ReadSSE(this Response<Stream> response, Action<Newtonsoft.Json.Linq.JObject> jEvents, CancellationToken token)
    {
        await response.ReadSSE(textEvents: (data) =>
        {
            if (string.IsNullOrEmpty(data)) return;
            try
            {
                jEvents(Newtonsoft.Json.Linq.JObject.Parse(data));
            }
            catch (Exception ex)
            {
                var jEv = Newtonsoft.Json.Linq.JObject.FromObject(new
                {
                    OriginalLine = data,
                    ExceptionMessage = ex.Message,
                    ExceptionContent = ex.ToString(),
                });
                jEvents(jEv);
            }
        }, token);
    }
    public static async Task ReadSSE(this Task<Response<Stream>> responseTask, Action<Newtonsoft.Json.Linq.JObject> jEvents, CancellationToken token)
    {
        await (await responseTask).ReadSSE(jEvents: jEvents, token);
    }
#endif
}
