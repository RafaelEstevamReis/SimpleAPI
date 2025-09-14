#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
namespace Simple.API;

using Simple.API.ClientBuilderAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

public class ClientBuilder : DispatchProxy
{
    static readonly Type TypeOfTask = typeof(Task);
    static readonly Type TypeOfClientInfo = typeof(ClientInfo);
    static readonly Type TypeOfResponseExtensions = typeof(ResponseExtensions);

    static readonly MethodInfo[] MethodsOfClientInfo = TypeOfClientInfo.GetMethods();
    static readonly MethodInfo MethodGetAsync = MethodsOfClientInfo.Where(o => o.Name == nameof(ClientInfo.GetAsync)).First();
    static readonly MethodInfo MethodPostAsync = MethodsOfClientInfo.Where(o => o.Name == nameof(ClientInfo.PostAsync)).First();
    static readonly MethodInfo MethodPutAsync = MethodsOfClientInfo.Where(o => o.Name == nameof(ClientInfo.PutAsync)).First();

    static readonly MethodInfo MethodGetSuccessfulDataTask = TypeOfResponseExtensions
        .GetMethods()
        .Where(o => o.Name == nameof(ResponseExtensions.GetSuccessfulData)
                    && o.ReturnType.BaseType.Name == "Task")
        .First(); // Exception if not found




    internal ClientInfo client;
    public ClientBuilder()
    {
    }

    public static T Create<T>(string uri, HttpMessageHandler clientHandler = null)
        where T : class
    {
        var proxy = Create<T, ClientBuilder>();
        var client = new ClientInfo(uri, clientHandler);
        (proxy as ClientBuilder).client = client;

        var typeT = typeof(T);
        var timeoutAttr = typeT.GetCustomAttribute<TimeoutAttribute>();
        if (timeoutAttr != null)
        {
            client.Timeout = timeoutAttr.Timeout;
        }

        return proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // Internal Methods
        if (processInternalFunctions(targetMethod, args, out object intRet))
        {
            return intRet;
        }

        /* Validations */
        MethodAttribute httpMethod = getHttpMethodAttribute(targetMethod);
        if (httpMethod == null) throw new InvalidOperationException($"Method {targetMethod.Name} lacks MethodAttribute");

        // Ensure the method returns a Task<T>
        if (!TypeOfTask.IsAssignableFrom(targetMethod.ReturnType)) throw new InvalidOperationException($"Method {targetMethod.Name} must return a Task");

        // Check InRoute
        var methodParams = targetMethod.GetParameters();
        var inRoutes = methodParams.Select(o => o.GetCustomAttribute<InRouteAttribute>()).ToArray();
        var hasAnyInRoutes = inRoutes.Any(o => o != null);
        var hasRouteParam = httpMethod.Route.Contains('{');
        if (hasAnyInRoutes && !hasRouteParam) throw new InvalidOperationException($"Attribute {nameof(InRouteAttribute)} must be used in a route with parameters");
        if (!hasAnyInRoutes && hasRouteParam) throw new InvalidOperationException($"Route parameters must be used with {nameof(InRouteAttribute)}");

        /* Return Type */
        // Get the return type (e.g., Response<TestResponse> from Task<Response<TestResponse>>)
        var taskReturnType = targetMethod.ReturnType.IsGenericType ? targetMethod.ReturnType.GetGenericArguments()[0] : null;

        // Detect if Response<T> or <T>
        Type innerType;
        bool usesResponseReturn;
        if (taskReturnType == null || !taskReturnType.IsGenericType || taskReturnType.GetGenericTypeDefinition() != typeof(Response<>))
        {
            // Returns Task<T>, will call GetSuccessfulData<T>()
            innerType = taskReturnType;
            usesResponseReturn = false;
        }
        else
        {
            // Extract the inner type T (e.g., TestResponse from Response<TestResponse>)
            innerType = taskReturnType.GetGenericArguments()[0];
            usesResponseReturn = true;
        }

        /* Extract Route information */
        string route = httpMethod.Route;
        if (hasRouteParam)
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
            List<object> lstParams = new List<object>(args);
            for (int i = lstParams.Count - 1; i >= 0; i--)
            {
                if (inRoutes[i] != null) lstParams.RemoveAt(i);
            }
            args = lstParams.ToArray();
        }

        /* Method Selection */
        selectMethodToExecute(route, args, httpMethod, innerType, out MethodInfo methodToCall, out object[] methodArgs);
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

    private static void selectMethodToExecute(string route, object[] args, MethodAttribute httpMethod, Type innerType, out MethodInfo methodToCall, out object[] methodArgs)
    {
        if (httpMethod is GetAttribute)
        {
            methodToCall = MethodGetAsync.MakeGenericMethod(innerType);

            if (args.Length == 0) methodArgs = [route];
            else methodArgs = [route, args[0]];
        }
        else if (httpMethod is PostAttribute)
        {
            methodToCall = MethodPostAsync.MakeGenericMethod(innerType);

            if (args.Length == 0) methodArgs = [route, null];
            else methodArgs = [route, args[0]];
        }
        else if (httpMethod is PutAttribute)
        {
            methodToCall = MethodPutAsync.MakeGenericMethod(innerType);

            if (args.Length == 0) methodArgs = [route, null];
            else methodArgs = [route, args[0]];
        }
        else throw new NotSupportedException("HttpMethod not supported");
    }

    private static MethodAttribute getHttpMethodAttribute(MethodInfo targetMethod)
    {
        // Get the [METHOD] attribute
        MethodAttribute getAttr = targetMethod.GetCustomAttribute<GetAttribute>();
        MethodAttribute postAttr = targetMethod.GetCustomAttribute<PostAttribute>();
        MethodAttribute putAttr = targetMethod.GetCustomAttribute<PutAttribute>();
        MethodAttribute deleteAttr = targetMethod.GetCustomAttribute<DeleteAttribute>();

        return getAttr ?? postAttr ?? putAttr ?? deleteAttr;
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
            if (args.Length != 1) throw new ArgumentException("Expcted `string` bearer parameter");

            client.SetAuthorizationBearer((string)args[0]);
            intRet = null; // void
            return true;
        }
        if (targetMethod.Name == "SetHeader")
        {
            if (args.Length != 2) throw new ArgumentException("Expcted `string` header key and value parameters");

            client.SetHeader((string)args[0], (string)args[1]);
            intRet = null; // void
            return true;
        }

        // No more internals
        intRet = null;
        return false;
    }

}

#endif