using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace chatserver
{
    [KnownType(typeof(Models.User))]
    [KnownType(typeof(Models.Message))]
    public class ChatWebSocketMessage
    {
        public string action { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
        public dynamic data { get; set; }

        public ChatWebSocketMessage()
        {
            message = String.Empty;
            timestamp = DateTime.UtcNow;
        }
    }

    public class ChatWebSocket
    {

        public static byte[] buffer = new byte[1024 * 4];

        private static readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public static async Task Listen(HttpContext context)
        {
            CancellationToken ct = context.RequestAborted;
            WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
            var socketId = Guid.NewGuid().ToString();

            if (_sockets.TryAdd(socketId, currentSocket))
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    string message = null;
                    ArraySegment<byte> bufferSegment = new ArraySegment<byte>(buffer);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        WebSocketReceiveResult result;
                        try
                        {
                            do
                            {
                                result = await currentSocket.ReceiveAsync(bufferSegment, ct);
                                ms.Write(bufferSegment.Array, bufferSegment.Offset, result.Count);
                            }
                            while (!result.EndOfMessage);
                        }
                        catch
                        {
                            break;
                        }

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            message = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }

                    if (currentSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        Send(new ChatWebSocketMessage()
                        {
                            action = "echo",
                            message = message
                        });
                    }
                }
            }

            _sockets.TryRemove(socketId, out _);
            try
            {
                await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to close websocket from client " + socketId + ": " + e.Message);
            }
        }

        public static void Send(ChatWebSocketMessage message)
        {
            Newtonsoft.Json.JsonSerializerSettings jsonsettings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
            };
            jsonsettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            byte[] jsonbyte = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message, jsonsettings));

            Task[] sendtasks = new Task[_sockets.Count];
            int socketcounter = 0;
            Console.WriteLine("Going to send a message to " + _sockets.Count + " clients");
            foreach (WebSocket socket in _sockets.Values)
            {
                sendtasks[socketcounter] = Task.CompletedTask;
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        sendtasks[socketcounter] = socket.SendAsync(new ArraySegment<byte>(jsonbyte), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to send message to client " + socketcounter + " with state " + socket.State + ": " + e.Message);
                        continue;
                    }
                }
                socketcounter++;
            }

            Task.WaitAll(sendtasks);
        }
    }

}