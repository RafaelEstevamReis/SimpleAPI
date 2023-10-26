#if !NETSTANDARD1_1

using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace Simple.API.WebSocketProcessors;

public class StringDataProcessor : WebSocketProcessorBase<string, string>
{
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public override string ProcessReceivedData(Stream result)
    {
        using StreamReader reader = new StreamReader(result, Encoding);
        return reader.ReadToEnd();
    }
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(string data)
    {
        var bytes = Encoding.GetBytes(data);
        return (new ArraySegment<byte>(bytes), WebSocketMessageType.Text);
    }
    public override ArraySegment<byte> ProcessClose()
    {
#if NETSTANDARD2_0
        return new ArraySegment<byte>(Array.Empty<byte>());
#else
        return ArraySegment<byte>.Empty;
#endif
    }
}

#endif