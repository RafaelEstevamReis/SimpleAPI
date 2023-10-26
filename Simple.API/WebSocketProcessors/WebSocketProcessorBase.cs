#if !NETSTANDARD1_1
namespace Simple.API.WebSocketProcessors;

using System;
using System.IO;
using System.Net.WebSockets;

public abstract class WebSocketProcessorBase<TSend, TReceive>
{
    public abstract TReceive ProcessReceivedData(Stream result);
    public abstract (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data);
    public abstract ArraySegment<byte> ProcessClose();
}
#endif
