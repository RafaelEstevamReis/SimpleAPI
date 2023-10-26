#if !NETSTANDARD1_1

using System;
using System.IO;
using System.Net.WebSockets;

namespace Simple.API.WebSocketProcessors;

public class GenericDataProcessor<TSend, TReceive> : WebSocketProcessorBase<TSend, TReceive>
{
    public GenericDataProcessor(
        Func<Stream, TReceive> receiveAction,
        Func<TSend, (ArraySegment<byte>, WebSocketMessageType)> sendAction,
        Func<ArraySegment<byte>> closeAction
        )
    {
        ReceiveAction = receiveAction;
        SendAction = sendAction;
        CloseAction = closeAction;
    }

    public Func<Stream, TReceive> ReceiveAction { get; }
    public Func<TSend, (ArraySegment<byte>, WebSocketMessageType)> SendAction { get; }
    public Func<ArraySegment<byte>> CloseAction { get; }

    public override TReceive ProcessReceivedData(Stream result)
        => ReceiveAction(result);
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data)
        => SendAction(data);
    public override ArraySegment<byte> ProcessClose()
        => CloseAction();
}

#endif