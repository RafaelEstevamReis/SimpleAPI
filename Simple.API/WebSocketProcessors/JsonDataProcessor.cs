#if !NETSTANDARD1_1
namespace Simple.API.WebSocketProcessors;

using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Processor that serializes messages as JSON (UTF-8 by default); when the type is string the raw text is used
/// </summary>
/// <typeparam name="TSend">Type of messages sent</typeparam>
/// <typeparam name="TReceive">Type of messages received</typeparam>
public class JsonDataProcessor<TSend, TReceive> : WebSocketProcessorBase<TSend, TReceive>
{
    /// <summary>
    /// Text encoding used to read and write messages (default UTF-8)
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <inheritdoc/>
    public override TReceive ProcessReceivedData(Stream result)
    {
        using StreamReader reader = new StreamReader(result, Encoding);
        var json = reader.ReadToEnd();
        if (typeof(TReceive) == typeof(string)) return (TReceive)(object)json;
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TReceive>(json);
    }
    /// <inheritdoc/>
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data)
    {
        string json;
        if (typeof(TSend) == typeof(string)) json = data.ToString();
        else json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

        var bytes = Encoding.GetBytes(json);
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