#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
namespace Simple.API;

using Simple.API.ClientBuilderAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Creates a new Client instance
/// </summary>
public class ClientBuilder : DispatchProxy
{
    static readonly Type TypeOfTask = typeof(Task);

    static readonly MethodInfo MethodGetAsync = resolve(nameof(ClientInfo.GetAsync), true, typeof(string));
    static readonly MethodInfo MethodPostAsync = resolve(nameof(ClientInfo.PostAsync), true, typeof(string), typeof(object));
    static readonly MethodInfo MethodPutAsync = resolve(nameof(ClientInfo.PutAsync), true, typeof(string), typeof(object));
    static readonly MethodInfo MethodPatchAsync = resolve(nameof(ClientInfo.PatchAsync), true, typeof(string), typeof(object));
    static readonly MethodInfo MethodDeleteAsync = resolve(nameof(ClientInfo.DeleteAsync), true, typeof(string));
    // Non-generic (Response only, no typed body) overloads, used for Task<Response> returns.
    static readonly MethodInfo MethodGetAsyncNoData = resolve(nameof(ClientInfo.GetAsync), false, typeof(string));
    static readonly MethodInfo MethodPostAsyncNoData = resolve(nameof(ClientInfo.PostAsync), false, typeof(string), typeof(object));
    static readonly MethodInfo MethodPutAsyncNoData = resolve(nameof(ClientInfo.PutAsync), false, typeof(string), typeof(object));
    static readonly MethodInfo MethodPatchAsyncNoData = resolve(nameof(ClientInfo.PatchAsync), false, typeof(string), typeof(object));
    static readonly MethodInfo MethodDeleteAsyncNoData = resolve(nameof(ClientInfo.DeleteAsync), false, typeof(string));
    // Resolves a ClientInfo overload by exact (generic-ness + parameter) signature,
    // instead of relying on GetMethods() declaration order.
    static MethodInfo resolve(string name, bool generic, params Type[] paramTypes)
        => typeof(ClientInfo).GetMethods().Single(o => o.Name == name && o.IsGenericMethod == generic
            && o.GetParameters().Select(p => p.ParameterType).SequenceEqual(paramTypes));

    static readonly Type TypeOfResponseExtensions = typeof(ResponseExtensions);
    static readonly MethodInfo MethodGetSuccessfulDataTask = TypeOfResponseExtensions
        .GetMethods()
        .Where(o => o.Name == nameof(ResponseExtensions.GetSuccessfulData)
                    && o.ReturnType.BaseType.Name == "Task")
        .First(); // Exception if not found

    internal ClientInfo client;

    /// <summary>
    /// Creates a new instance for the specified Interface
    /// </summary>
    public static T Create<T>(string uri, HttpMessageHandler clientHandler = null)
        where T : class
    {
        // Base ClientInfo
        var client = new ClientInfo(uri, clientHandler);

        // Process Interface Attributes
        var typeT = typeof(T);
        var timeoutAttr = typeT.GetCustomAttribute<TimeoutAttribute>();
        if (timeoutAttr != null)
        {
            client.Timeout = timeoutAttr.Timeout;
        }

        // Create proxy
        var proxy = DispatchProxy.Create<T, ClientBuilder>();
        (proxy as ClientBuilder).client = client;

        return proxy;
    }

    /// <summary>
    /// Whenever any method on the generated proxy type is called, this method
    /// will be invoked to dispatch control.
    /// </summary>
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        /* Validations */
        MethodAttribute httpMethod = getHttpMethodAttribute(targetMethod);
        if (httpMethod == null)
        {
            // No HTTP attribute -> internal helper (GetInternalClient / SetAuthorizationBearer / SetHeader).
            // HTTP attributes take precedence, so a mapped method named like an internal is not hijacked.
            if (processInternalFunctions(targetMethod, args, out object intRet)) return intRet;
            throw new InvalidOperationException($"Method {targetMethod.Name} lacks MethodAttribute");
        }

        // Ensure the method returns a Task
        if (!TypeOfTask.IsAssignableFrom(targetMethod.ReturnType)) throw new InvalidOperationException($"Method {targetMethod.Name} must return a Task");

        // Check InRoute
        var methodParams = targetMethod.GetParameters();
        var inRoutesParams = methodParams.Select(o => o.GetCustomAttribute<InRouteAttribute>()).ToArray();
        var hasAnyInRoutes = inRoutesParams.Any(o => o != null);
        var hasRouteParam = httpMethod.Route.Contains('{');
        if (hasAnyInRoutes && !hasRouteParam) throw new InvalidOperationException($"Attribute {nameof(InRouteAttribute)} must be used in a route with parameters");
        if (!hasAnyInRoutes && hasRouteParam) throw new InvalidOperationException($"Route parameters must be used with {nameof(InRouteAttribute)}");

        /* Return Type */
        // Get the return type (e.g., Response<TestResponse> from Task<Response<TestResponse>>)
        var taskReturnType = targetMethod.ReturnType.IsGenericType ? targetMethod.ReturnType.GetGenericArguments()[0] : null;

        // Detect if Task<Response<T>>, Task<Response>, Task<T>, or plain Task
        Type innerType;
        bool usesResponseReturn;
        bool noBody = false;
        if (taskReturnType != null && taskReturnType.IsGenericType && taskReturnType.GetGenericTypeDefinition() == typeof(Response<>))
        {
            // Task<Response<T>>: return the Response<T> untouched
            innerType = taskReturnType.GetGenericArguments()[0];
            usesResponseReturn = true;
        }
        else if (taskReturnType == typeof(Response))
        {
            // Task<Response>: metadata only. Uses the non-generic overload, so the body is
            // not deserialized and the caller inspects Response.IsSuccessStatusCode itself.
            innerType = null;
            usesResponseReturn = true;
            noBody = true;
        }
        else
        {
            // Task<T> (unwrap via GetSuccessfulData) or plain Task (no body -> discard as object)
            innerType = taskReturnType ?? typeof(object);
            usesResponseReturn = false;
        }

        /* Extract Route parameters */
        string route = httpMethod.Route;
        if (hasRouteParam)
        {
            processInRouteArguments(methodParams, inRoutesParams, ref route, ref args);
        }

        /* Method Selection */
        selectMethodToExecute(route, args, httpMethod, innerType, client.NullParameterHandlingPolicy_IgnoreNulls, noBody, out MethodInfo methodToCall, out object[] methodArgs);
        // Call ClientInfo.[Method]Async<T> dynamically with the inner type
        var task = (Task)methodToCall.Invoke(client, methodArgs);

        // Calls GetSuccessfulData
        if (!usesResponseReturn)
        {
            var getSuccessfulDataMethod = MethodGetSuccessfulDataTask.MakeGenericMethod(innerType);
            task = (Task)getSuccessfulDataMethod.Invoke(null, [task]);
        }

        return task;
    }

    private static void processInRouteArguments(ParameterInfo[] methodParams, InRouteAttribute[] inRoutes, ref string route, ref object[] args)
    {
        Dictionary<string, string> dicParams = [];
        // Parse route params
        for (int i = 0; i < inRoutes.Length; i++)
        {
            if (inRoutes[i] == null) continue;

            // Get 'Key' in [inRoutes] and 'value' in [args]
            var name = methodParams[i].Name;
            var value = args[i];

            dicParams[name] = value?.ToString() ?? "";
        }

        // Replace route
        foreach (var pair in dicParams)
        {
            string key = $"{{{pair.Key}}}";
            route = route.Replace(key, pair.Value);
        }

        // rebuild args
        List<object> lstParams = [.. args];
        for (int i = lstParams.Count - 1; i >= 0; i--)
        {
            if (inRoutes[i] != null) lstParams.RemoveAt(i);
        }
        args = lstParams.ToArray();
    }

    private static void selectMethodToExecute(string route, object[] args, MethodAttribute httpMethod, Type innerType, bool ignoreNulls, bool noBody, out MethodInfo methodToCall, out object[] methodArgs)
    {
        if (httpMethod is GetAttribute)
        {
            methodToCall = noBody ? MethodGetAsyncNoData : MethodGetAsync.MakeGenericMethod(innerType);
            // A leftover (non-[InRoute]) argument is serialized into the query string,
            // mirroring ClientInfoExtensions.GetAsync<T>(client, service, object).
            if (args.Length == 0) methodArgs = [route];
            else methodArgs = [Helper.buildUrl(route, Helper.buildParams(args[0], ignoreNulls))];
        }
        else if (httpMethod is PostAttribute)
        {
            methodToCall = noBody ? MethodPostAsyncNoData : MethodPostAsync.MakeGenericMethod(innerType);
            methodArgs = args.Length == 0 ? [route, null] : [route, args[0]];
        }
        else if (httpMethod is PutAttribute)
        {
            methodToCall = noBody ? MethodPutAsyncNoData : MethodPutAsync.MakeGenericMethod(innerType);
            methodArgs = args.Length == 0 ? [route, null] : [route, args[0]];
        }
        else if (httpMethod is PatchAttribute)
        {
            methodToCall = noBody ? MethodPatchAsyncNoData : MethodPatchAsync.MakeGenericMethod(innerType);
            methodArgs = args.Length == 0 ? [route, null] : [route, args[0]];
        }
        else if (httpMethod is DeleteAttribute)
        {
            methodToCall = noBody ? MethodDeleteAsyncNoData : MethodDeleteAsync.MakeGenericMethod(innerType);
            methodArgs = [route]; // ClientInfo.DeleteAsync<T> takes only the service route
        }
        else throw new NotSupportedException("HttpMethod not supported");
    }

    private static MethodAttribute getHttpMethodAttribute(MethodInfo targetMethod)
    {
        // Get the [METHOD] attribute
        MethodAttribute getAttr = targetMethod.GetCustomAttribute<GetAttribute>();
        MethodAttribute postAttr = targetMethod.GetCustomAttribute<PostAttribute>();
        MethodAttribute putAttr = targetMethod.GetCustomAttribute<PutAttribute>();
        MethodAttribute patchAttr = targetMethod.GetCustomAttribute<PatchAttribute>();
        MethodAttribute deleteAttr = targetMethod.GetCustomAttribute<DeleteAttribute>();

        return getAttr ?? postAttr ?? putAttr ?? patchAttr ?? deleteAttr;
    }

    private bool processInternalFunctions(MethodInfo targetMethod, object[] args, out object intRet)
    {
        // Process Internal Functions
        if (targetMethod.Name == "GetInternalClient")
        {
            intRet = client;
            return true;
        }
        if (targetMethod.Name == "SetAuthorizationBearer")
        {
            if (args.Length != 1) throw new ArgumentException("Expected `string` bearer parameter");

            client.SetAuthorizationBearer((string)args[0]);
            intRet = null; // void
            return true;
        }
        if (targetMethod.Name == "SetHeader")
        {
            if (args.Length != 2) throw new ArgumentException("Expected `string` header key and value parameters");

            client.SetHeader((string)args[0], (string)args[1]);
            intRet = null; // void
            return true;
        }

        // No more internals
        intRet = null;
        return false;
    }

}

/// <summary>
/// Optional interface exposing the built client's internal helpers, which are not mapped to HTTP calls.
/// Inherit it on your API interface to reach the underlying client and the auth/header shortcuts.
/// </summary>
public interface IBuildedClientInternalFunctions
{
    /// <summary>
    /// Gets the underlying <see cref="ClientInfo"/> instance backing the generated client
    /// </summary>
    ClientInfo GetInternalClient();
    /// <summary>
    /// Sets the Authorization header with a Bearer token
    /// </summary>
    /// <param name="bearer">Bearer token value (without the "Bearer " prefix)</param>
    void SetAuthorizationBearer(string bearer);
    /// <summary>
    /// Sets a request header, replacing any existing value
    /// </summary>
    /// <param name="key">Header name</param>
    /// <param name="value">Header value</param>
    void SetHeader(string key, string value);
}

#endif