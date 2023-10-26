using Simple.API.WebSocketProcessors;
using System;
using System.Threading.Tasks;

namespace Simple.Test
{
    class Example_WebSocket
    {
        public static async Task Run_String()
        {
            string url = "";

            var stringDataProcessor = new StringDataProcessor();
            var ws = new API.WebSocket<string, string>(url, stringDataProcessor);

            ws.OnConnectionClosed += Ws_OnConnectionClosed;
            ws.OnMessageReceived += Ws_OnMessageReceived;
            ws.OnError += Ws_OnError;

            await ws.ConnectAsync();

            await ws.SendMessageAsync("Hello");

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            await ws.DisconnectAsync();

            await Task.Delay(5000);
        }

        public static async Task Run_Json()
        {
            string url = "";

            var json = new JsonDataProcessor<string, R>();
            var ws = new API.WebSocket<string, R>(url, json);

            ws.OnConnectionClosed += Ws_OnConnectionClosed;
            ws.OnMessageReceived += Ws_OnMessageReceived;
            ws.OnError += Ws_OnError;

            await ws.ConnectAsync();

            await ws.SendMessageAsync("Hello");

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            await ws.DisconnectAsync();
        }

        private static void Ws_OnMessageReceived<T>(object sender, T e)
        {
            Console.WriteLine(e);
        }
        private static void Ws_OnError(object sender, Exception e)
        {
            Console.WriteLine(e);
        }
        private static void Ws_OnConnectionClosed(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            Console.WriteLine("[Closed]");
        }

    }
    public record R
    {
        public string reqFuncName { get; set; }
        public string operationID { get; set; }
        public string data { get; set; }
    }
}
