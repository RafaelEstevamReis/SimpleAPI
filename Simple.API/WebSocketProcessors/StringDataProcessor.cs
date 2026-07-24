#if !NETSTANDARD1_1

using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace Simple.API.WebSocketProcessors;

/// <summary>
/// Processor for plain string messages (UTF-8 by default)
/// </summary>
public class StringDataProcessor : WebSocketProcessorBase<string, string>
{
    /// <summary>
    /// Text encoding used to read and write messages (default UTF-8)
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <inheritdoc/>
    public override string ProcessReceivedData(Stream result)
    {
        using StreamReader reader = new StreamReader(result, Encoding);
        return reader.ReadToEnd();
    }
    /// <inheritdoc/>
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(string data)
    {
        var bytes = Encoding.GetBytes(data);
        return (new ArraySegment<byte>(bytes), WebSocketMessageType.Text);
    }
    /// <inheritdoc/>
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