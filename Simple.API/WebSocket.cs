#if !NETSTANDARD1_1
namespace Simple.API;

using Simple.API.WebSocketProcessors;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class WebSocket<TSend, TReceive> : IDisposable
#if !NETSTANDARD2_0
    , IAsyncDisposable
#endif
{
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancelSource;

    public string Url { get; }
    public WebSocketProcessorBase<TSend, TReceive> Processor { get; }
    public int ReceiveBufferSize { get; set; } = 4 * 1024; // 4KB

    public event EventHandler<TReceive> OnMessageReceived;
    public event EventHandler<WebSocketCloseStatus> OnConnectionClosed;
    public event EventHandler<Exception> OnError;

    public ClientWebSocket InternalClient => webSocket;

    public WebSocket(string url, WebSocketProcessorBase<TSend, TReceive> processor)
    {
        Url = url;
        webSocket = new ClientWebSocket();
        Processor = processor;
    }

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

    public async Task SendMessageAsync(TSend data)
        => await SendMessageAsync(data, cancelSource?.Token ?? CancellationToken.None);

    public async Task SendMessageAsync(TSend data, CancellationToken cancellationToken)
    {
        ensureConnected();
        var d = Processor.ProcessSendData(data);
        await webSocket.SendAsync(d.Item1, d.Item2, true, cancellationToken);
    }
    public async Task SendCloseMessageAsync()
        => await SendCloseMessageAsync(cancelSource?.Token ?? CancellationToken.None);
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

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

#if !NETSTANDARD2_0
    public async System.Threading.Tasks.ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
#endif

}
#endif