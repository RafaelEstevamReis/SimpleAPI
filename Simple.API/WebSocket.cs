#if !NETSTANDARD1_1
namespace Simple.API;

using Simple.API.WebSocketProcessors;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class WebSocket<TSend, TReceive>
{
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancelSource;

    public string Url { get; }
    public WebSocketProcessorBase<TSend, TReceive> Processor { get; }
    public int ReceiveBufferSize { get; set; } = 4 * 1024; // 4KB
    public TimeSpan DisconnectWaitTime { get; set; } = TimeSpan.FromMilliseconds(750);

    public event EventHandler<TReceive> OnMessageReceived;
    public event EventHandler<WebSocketCloseStatus> OnConnectionClosed;
    public event EventHandler<Exception> OnError;

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
    public async Task DisconnectAsync()
    {
        if (webSocket is null) return;
        if (webSocket.State == WebSocketState.Open)
        {
            cancelSource.CancelAfter(DisconnectWaitTime);

            await webSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
        webSocket.Dispose();
        webSocket = null;

        cancelSource.Dispose();
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
                    OnConnectionClosed?.Invoke(this, receiveResult.CloseStatus ?? WebSocketCloseStatus.Empty);
                    break;
                }
                outputStream.Position = 0;
                responseReceived(outputStream);
            }
        }
        catch (TaskCanceledException) { /**/ }
        catch(Exception ex)
        {
            if (OnError == null) throw;

            OnError.Invoke(this, ex);
        }
        finally
        {
            outputStream?.Dispose();
            await DisconnectAsync();
        }
    }

    public async Task SendMessageAsync(TSend data)
    {
        await SendMessageAsync(data, cancelSource.Token);
    }
    public async Task SendMessageAsync(TSend data, CancellationToken cancellationToken)
    {
        var d = Processor.ProcessSendData(data);
        await webSocket.SendAsync(d.Item1, d.Item2, d.Item2 == WebSocketMessageType.Close, cancellationToken);
    }
    public async Task SendCloseMessageAsync( CancellationToken cancellationToken)
    {
        var d = Processor.ProcessClose();
        await webSocket.SendAsync(d,  WebSocketMessageType.Close, true, cancellationToken);
    }
    private void responseReceived(Stream inputStream)
    {
        var data = Processor.ProcessReceivedData(inputStream);
        OnMessageReceived?.Invoke(this, data);
    }

    public void Dispose() => DisconnectAsync().Wait();

}
#endif