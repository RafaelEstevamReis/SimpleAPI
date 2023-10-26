#if !NETSTANDARD1_1
namespace Simple.API;

using Simple.API.WebSocketProcessors;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

internal class WebSocket<DataTypeSend, DataTypeReceive>
{
    public int ReceiveBufferSize { get; set; } = 4 * 1024; // 4KB
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancelSource;

    public string Url { get; }
    public WebSocketProcessorBase<DataTypeSend, DataTypeReceive> Processor { get; }

    public event EventHandler<DataTypeReceive> OnMessageReceived;
    public event EventHandler<WebSocketCloseStatus> OnConnectionClosed;

    public WebSocket(string url, WebSocketProcessorBase<DataTypeSend, DataTypeReceive> processor)
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
            cancelSource.CancelAfter(TimeSpan.FromMilliseconds(250));
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
        MemoryStream outputStream = null;
        WebSocketReceiveResult receiveResult = null;
        var buffer = new ArraySegment<byte>(new byte[ReceiveBufferSize]);
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
        finally
        {
            outputStream?.Dispose();
        }
    }

    public async Task SendMessageAsync<RequestType>(DataTypeSend data, CancellationToken cancellationToken)
    {
        var d = Processor.ProcessSendData(data);
        await webSocket.SendAsync(d.Item1, d.Item2, d.Item2 == WebSocketMessageType.Close, cancellationToken);
    }
    public async Task SendCloseMessageAsync<RequestType>( CancellationToken cancellationToken)
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