#if !NETSTANDARD1_1

using System;
using System.IO;
using System.Net.WebSockets;

namespace Simple.API.WebSocketProcessors;

/// <summary>
/// Processor whose behavior is supplied by delegates, for custom (de)serialization
/// </summary>
/// <typeparam name="TSend">Type of messages sent</typeparam>
/// <typeparam name="TReceive">Type of messages received</typeparam>
public class GenericDataProcessor<TSend, TReceive> : WebSocketProcessorBase<TSend, TReceive>
{
    /// <summary>
    /// Creates a processor from the given delegates
    /// </summary>
    /// <param name="receiveAction">Converts a received stream into a value</param>
    /// <param name="sendAction">Converts a value into the bytes and message type to send</param>
    /// <param name="closeAction">Produces the close-frame payload</param>
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

    /// <summary>
    /// Delegate that converts a received stream into a value
    /// </summary>
    public Func<Stream, TReceive> ReceiveAction { get; }
    /// <summary>
    /// Delegate that converts a value into the bytes and message type to send
    /// </summary>
    public Func<TSend, (ArraySegment<byte>, WebSocketMessageType)> SendAction { get; }
    /// <summary>
    /// Delegate that produces the close-frame payload
    /// </summary>
    public Func<ArraySegment<byte>> CloseAction { get; }

    /// <inheritdoc/>
    public override TReceive ProcessReceivedData(Stream result)
        => ReceiveAction(result);
    /// <inheritdoc/>
    public override (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data)
        => SendAction(data);
    /// <inheritdoc/>
    public override ArraySegment<byte> ProcessClose()
        => CloseAction();
}

#endif