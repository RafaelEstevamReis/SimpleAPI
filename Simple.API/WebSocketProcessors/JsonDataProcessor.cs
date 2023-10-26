#if !NETSTANDARD1_1
namespace Simple.API.WebSocketProcessors;

using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;

public class JsonDataProcessor<TSend, TReceive> : WebSocketProcessorBase<TSend, TReceive>
{
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    public override TReceive ProcessReceivedData(Stream result)
    {
        using StreamReader reader = new StreamReader(result, Encoding);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TReceive>(reader.ReadToEnd());
    }
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var bytes = Encoding.GetBytes(json);
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