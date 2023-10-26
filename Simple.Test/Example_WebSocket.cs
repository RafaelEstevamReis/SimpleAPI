using Simple.API.WebSocketProcessors;
using System;
using System.Threading.Tasks;

namespace Simple.Test
{
    class Example_WebSocket
    {
        public static async Task Run()
        {
            string url = "";

            var stringDataProcessor = new StringDataProcessor();
            var ws = new API.WebSocket<string, string>(url, stringDataProcessor);

            ws.OnConnectionClosed += Ws_OnConnectionClosed;
            ws.OnMessageReceived += Ws_OnMessageReceived;

            await ws.ConnectAsync();

            await ws.SendMessageAsync("Hello");

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            await ws.DisconnectAsync();
        }

        private static void Ws_OnMessageReceived(object sender, string e)
        {
            Console.WriteLine(e);
        }
        private static void Ws_OnConnectionClosed(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            Console.WriteLine("[Closed]");
        }
    }
}
