namespace Simple.API;

using System.IO;
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
#endif
}
