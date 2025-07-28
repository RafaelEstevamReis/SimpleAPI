#if NET6_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
namespace Simple.API;

using Simple.API.ClientBuilderAttributes;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public class ClientBuilder : DispatchProxy
{
    static readonly Type TypeOfTask = typeof(Task);
    static readonly Type TypeOfClientInfo = typeof(ClientInfo);
    static readonly Type TypeOfResponseExtensions = typeof(ResponseExtensions);

    static readonly MethodInfo[] MethodsOfClientInfo = TypeOfClientInfo.GetMethods();
    static readonly MethodInfo[] MethodsOfResponseExtensions = TypeOfResponseExtensions.GetMethods();

    internal ClientInfo client;
    public ClientBuilder()
    {
    }

    public static T Create<T>(string uri)
        where T : class
    {
        var proxy = Create<T, ClientBuilder>();
        (proxy as ClientBuilder).client = new ClientInfo(uri); // Pass Uri to ClientInfo
        return proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // Get the [METHOD] attribute
        MethodAttribute getAttr = targetMethod.GetCustomAttribute<GetAttribute>();
        MethodAttribute postAttr = targetMethod.GetCustomAttribute<PostAttribute>();
        MethodAttribute putAttr = targetMethod.GetCustomAttribute<PutAttribute>();
        //MethodAttribute deleteAttr = targetMethod.GetCustomAttribute<DeleteAttribute>();

        MethodAttribute httpMethod = getAttr
                                    ?? postAttr
                                    ?? putAttr
                                    //?? deleteAttr
                                    ;
        if (httpMethod == null) throw new InvalidOperationException($"Method {targetMethod.Name} lacks MethodAttribute");

        // Ensure the method returns a Task<T>
        if (!TypeOfTask.IsAssignableFrom(targetMethod.ReturnType)) throw new InvalidOperationException($"Method {targetMethod.Name} must return a Task");

        // Get the return type (e.g., Response<TestResponse> from Task<Response<TestResponse>>)
        var taskReturnType = targetMethod.ReturnType.IsGenericType ? targetMethod.ReturnType.GetGenericArguments()[0] : null;

        // Detect if RESPONSE or DIRECT
        Type innerType;
        bool usesResponseReturn;
        if (taskReturnType == null || !taskReturnType.IsGenericType || taskReturnType.GetGenericTypeDefinition() != typeof(Response<>))
        {
            innerType = taskReturnType;
            usesResponseReturn = false;
        }
        else
        {
            // Extract the inner type T (e.g., TestResponse from Response<TestResponse>)
            innerType = taskReturnType.GetGenericArguments()[0];
            usesResponseReturn = true;
        }

        // Call ClientInfo.GetAsync<T> dynamically with the inner type
        MethodInfo methodToCall;
        object[] methodArgs;

        if (getAttr != null)
        {
            var allMethods = MethodsOfClientInfo.Where(o => o.Name == nameof(ClientInfo.GetAsync)).ToArray();
            methodToCall = allMethods.FirstOrDefault().MakeGenericMethod(innerType);

            if (args.Length == 0) methodArgs = [httpMethod.Route, null];
            else methodArgs = [httpMethod.Route, args[0]];
        }
        else if (postAttr != null)
        {
            var allMethods = MethodsOfClientInfo.Where(o => o.Name == nameof(ClientInfo.PostAsync)).ToArray();
            methodToCall = allMethods.FirstOrDefault().MakeGenericMethod(innerType);

            if (args.Length == 0) methodArgs = [httpMethod.Route, null];
            else methodArgs = [httpMethod.Route, args[0]];
        }
        else if (putAttr != null)
        {
            var allMethods = MethodsOfClientInfo.Where(o => o.Name == nameof(ClientInfo.PutAsync)).ToArray();
            methodToCall = allMethods.FirstOrDefault().MakeGenericMethod(innerType);

            if (args.Length == 0) methodArgs = [httpMethod.Route, null];
            else methodArgs = [httpMethod.Route, args[0]];
        }
        else throw new NotSupportedException("HttpMethod not supported");

        var task = (Task)methodToCall.Invoke(client, methodArgs);

        if (!usesResponseReturn)
        {
            var getSuccessfulDataMethod = MethodsOfResponseExtensions.FirstOrDefault(o => o.ReturnType.BaseType.Name == "Task").MakeGenericMethod(innerType);
            task = (Task)getSuccessfulDataMethod.Invoke(null, [task]);
        }

        return task;
    }
}

#endif