namespace Simple.API.ClientBuilderAttributes;

using System;


public class MethodAttribute : Attribute
{
    public string Route { get; protected set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class GetAttribute : MethodAttribute
{
    public GetAttribute(string route) => Route = route;
}
[AttributeUsage(AttributeTargets.Method)]
public class PostAttribute : MethodAttribute
{
    public PostAttribute(string route) => Route = route;
}

[AttributeUsage(AttributeTargets.Method)]
public class PutAttribute : MethodAttribute
{
    public PutAttribute(string route) => Route = route;
}
[AttributeUsage(AttributeTargets.Method)]
public class DeleteAttribute : MethodAttribute
{
    public DeleteAttribute(string route) => Route = route;
}