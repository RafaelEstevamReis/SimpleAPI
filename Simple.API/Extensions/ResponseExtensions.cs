namespace Simple.API;

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
}
