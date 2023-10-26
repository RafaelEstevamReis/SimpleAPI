#if !NETSTANDARD1_1

using System;
using System.IO;
using System.Net.WebSockets;

namespace Simple.API.WebSocketProcessors;

public class GenericDataProcessor<DataTypeSend, DataTypeReceive> : WebSocketProcessorBase<DataTypeSend, DataTypeReceive>
{
    public GenericDataProcessor(
        Func<Stream, DataTypeReceive> receiveAction,
        Func<DataTypeSend, (ArraySegment<byte>, WebSocketMessageType)> sendAction,
        Func<ArraySegment<byte>> closeAction
        )
    {
        ReceiveAction = receiveAction;
        SendAction = sendAction;
        CloseAction = closeAction;
    }

    public Func<Stream, DataTypeReceive> ReceiveAction { get; }
    public Func<DataTypeSend, (ArraySegment<byte>, WebSocketMessageType)> SendAction { get; }
    public Func<ArraySegment<byte>> CloseAction { get; }

    public override DataTypeReceive ProcessReceivedData(Stream result)
        => ReceiveAction(result);
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(DataTypeSend data)
        => SendAction(data);
    public override ArraySegment<byte> ProcessClose()
        => CloseAction();
}

#endif