#if !NETSTANDARD1_1
namespace Simple.API;

using Simple.API.WebSocketProcessors;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal typed wrapper over <see cref="ClientWebSocket"/>. Sends <typeparamref name="TSend"/> and
/// raises <typeparamref name="TReceive"/> messages via events, delegating (de)serialization to a processor.
/// </summary>
/// <typeparam name="TSend">Type of messages sent</typeparam>
/// <typeparam name="TReceive">Type of messages received</typeparam>
public class WebSocket<TSend, TReceive> : IDisposable
#if !NETSTANDARD2_0
    , IAsyncDisposable
#endif
{
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancelSource;

    /// <summary>
    /// WebSocket endpoint URL
    /// </summary>
    public string Url { get; }
    /// <summary>
    /// Processor that (de)serializes messages to and from the socket
    /// </summary>
    public WebSocketProcessorBase<TSend, TReceive> Processor { get; }
    /// <summary>
    /// Size, in bytes, of the receive buffer (default 4 KB)
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 4 * 1024; // 4KB

    /// <summary>
    /// Raised for each complete message received
    /// </summary>
    public event EventHandler<TReceive> OnMessageReceived;
    /// <summary>
    /// Raised when the server closes the connection, carrying the close status
    /// </summary>
    public event EventHandler<WebSocketCloseStatus> OnConnectionClosed;
    /// <summary>
    /// Raised when the receive loop fails; the connection is then closed and disposed
    /// </summary>
    public event EventHandler<Exception> OnError;

    /// <summary>
    /// The underlying <see cref="ClientWebSocket"/>, or null once disconnected
    /// </summary>
    public ClientWebSocket InternalClient => webSocket;

    /// <summary>
    /// Creates a new instance for the given URL and message processor
    /// </summary>
    /// <param name="url">WebSocket endpoint URL</param>
    /// <param name="processor">Processor that (de)serializes messages</param>
    public WebSocket(string url, WebSocketProcessorBase<TSend, TReceive> processor)
    {
        Url = url;
        webSocket = new ClientWebSocket();
        Processor = processor;
    }

    /// <summary>
    /// Opens the connection and starts the background receive loop
    /// </summary>
    public async Task ConnectAsync()
    {
        if (webSocket != null)
        {
            if (webSocket.State == WebSocketState.Open) return;
            else webSocket.Dispose();
        }
        webSocket = new ClientWebSocket();

        cancelSource?.Dispose();
        cancelSource = new CancellationTokenSource();

        await webSocket.ConnectAsync(new Uri(Url), cancelSource.Token);
        await Task.Factory.StartNew(receiveLoop, cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }
    /// <summary>
    /// Closes the connection (if open) and releases the socket and its resources. Idempotent.
    /// </summary>
    public virtual async Task DisconnectAsync()
    {
        if (webSocket is null) return;
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                cancelSource?.Cancel();
            }
            catch (OperationCanceledException) { }
        }
        webSocket.Dispose();
        webSocket = null;

        cancelSource?.Dispose();
        cancelSource = null;
    }
    private async Task receiveLoop()
    {
        var receiveToken = cancelSource.Token;
        WebSocketReceiveResult receiveResult = null;
        var buffer = new ArraySegment<byte>(new byte[ReceiveBufferSize]);
        MemoryStream outputStream = null;
        try
        {
            while (!receiveToken.IsCancellationRequested)
            {
                outputStream = new MemoryStream(ReceiveBufferSize);
                do
                {
                    receiveResult = await webSocket.ReceiveAsync(buffer, cancelSource.Token);
                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;

                    outputStream.Write(buffer.Array, buffer.Offset, receiveResult.Count);
                }
                while (!receiveResult.EndOfMessage);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    var closeStatus = receiveResult.CloseStatus ?? WebSocketCloseStatus.Empty;
                    // Complete the closing handshake: echo a Close frame back (we already got the server's)
                    if (webSocket.State == WebSocketState.CloseReceived)
                    {
                        try { await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None); }
                        catch (Exception) { /* best-effort */ }
                    }
                    OnConnectionClosed?.Invoke(this, closeStatus);
                    await DisconnectAsync(); // release resources (idempotent, null-safe)
                    break;
                }
                outputStream.Position = 0;
                responseReceived(outputStream);
            }
        }
        catch (TaskCanceledException) { /**/ }
        catch (OperationCanceledException) { /**/ }
        catch (Exception ex)
        {
            // Never rethrow on this orphaned background task. Surface via OnError when handled,
            // then always release resources so a handler-less failure closes cleanly instead of leaking.
            OnError?.Invoke(this, ex);
            await DisconnectAsync();
        }
        finally
        {
            outputStream?.Dispose();
        }
    }

    /// <summary>
    /// Sends a message using the connection's cancellation token
    /// </summary>
    /// <param name="data">Message to send</param>
    public async Task SendMessageAsync(TSend data)
        => await SendMessageAsync(data, cancelSource?.Token ?? CancellationToken.None);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="data">Message to send</param>
    /// <param name="cancellationToken">Token to cancel the send</param>
    /// <exception cref="InvalidOperationException">The socket is not connected</exception>
    public async Task SendMessageAsync(TSend data, CancellationToken cancellationToken)
    {
        ensureConnected();
        var d = Processor.ProcessSendData(data);
        await webSocket.SendAsync(d.Item1, d.Item2, true, cancellationToken);
    }
    /// <summary>
    /// Sends a close message using the connection's cancellation token
    /// </summary>
    public async Task SendCloseMessageAsync()
        => await SendCloseMessageAsync(cancelSource?.Token ?? CancellationToken.None);
    /// <summary>
    /// Sends a close message
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the send</param>
    /// <exception cref="InvalidOperationException">The socket is not connected</exception>
    public async Task SendCloseMessageAsync(CancellationToken cancellationToken)
    {
        ensureConnected();
        var d = Processor.ProcessClose();
        await webSocket.SendAsync(d, WebSocketMessageType.Close, true, cancellationToken);
    }

    private void ensureConnected()
    {
        if (webSocket is null || webSocket.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not connected. Call ConnectAsync first.");
    }
    private void responseReceived(Stream inputStream)
    {
        var data = Processor.ProcessReceivedData(inputStream);
        OnMessageReceived?.Invoke(this, data);
    }

    /// <summary>
    /// Synchronously closes and releases the connection. Prefer DisposeAsync where available.
    /// </summary>
    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Asynchronously closes and releases the connection without blocking
    /// </summary>
    public async System.Threading.Tasks.ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
#endif

}
#endif