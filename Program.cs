using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace WebSocket_Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string address = "http://localhost:5151/";
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(address);
            httpListener.Start();
            Console.WriteLine($"Serveur WebSocket démarré sur ws:{address}");

            while (true)
            {
                HttpListenerContext context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                    Console.WriteLine("Client connecté.");

                    WebSocket webSocket = webSocketContext.WebSocket;

                    await Task.WhenAll(ReceiveMessages(webSocket), SendMessages(webSocket));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        static async Task ReceiveMessages(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                List<string> messageContent = FormatMessage(message);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(messageContent[0]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(messageContent[1]);
                Console.WriteLine();    
            }
        }

        static async Task SendMessages(WebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                string message = Console.ReadLine();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        static List<string> FormatMessage(string message)
        {
            char delimiter = ':';
            List<string> result = new List<string>();
            int index = message.IndexOf(delimiter);

            if (index != -1)
            {
                result.Add(message.Substring(0, index));
                result.Add(message.Substring(index + 1));
            }
            return result;
        }
    }
}
