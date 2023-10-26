#if !NETSTANDARD1_1
namespace Simple.API.WebSocketProcessors;

using System;
using System.IO;
using System.Net.WebSockets;

public abstract class WebSocketProcessorBase<DataTypeSend, DataTypeReceive>
{
    public abstract DataTypeReceive ProcessReceivedData(Stream result);
    public abstract (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(DataTypeSend data);
    public abstract ArraySegment<byte> ProcessClose();
}
#endif
