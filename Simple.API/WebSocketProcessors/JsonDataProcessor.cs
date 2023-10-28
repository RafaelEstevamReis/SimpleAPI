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
        var json = reader.ReadToEnd();
        if (typeof(TReceive) == typeof(string)) return (TReceive)(object)json;
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TReceive>(json);
    }
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data)
    {
        string json;
        if (typeof(TReceive) == typeof(string)) json = data.ToString();
        else json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

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