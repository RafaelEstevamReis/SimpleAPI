#if !NETSTANDARD1_1
namespace Simple.API.WebSocketProcessors;

using System;
using System.IO;
using System.Net.WebSockets;

/// <summary>
/// Base contract that converts messages between the socket's bytes and typed values
/// </summary>
/// <typeparam name="TSend">Type of messages sent</typeparam>
/// <typeparam name="TReceive">Type of messages received</typeparam>
public abstract class WebSocketProcessorBase<TSend, TReceive>
{
    /// <summary>
    /// Converts a received message stream into a <typeparamref name="TReceive"/> value
    /// </summary>
    /// <param name="result">Stream holding the complete received message</param>
    public abstract TReceive ProcessReceivedData(Stream result);
    /// <summary>
    /// Converts a value into the bytes and WebSocket message type to send
    /// </summary>
    /// <param name="data">Message to send</param>
    public abstract (ArraySegment<byte>, WebSocketMessageType) ProcessSendData(TSend data);
    /// <summary>
    /// Produces the payload sent with a close frame
    /// </summary>
    public abstract ArraySegment<byte> ProcessClose();
}
#endif
