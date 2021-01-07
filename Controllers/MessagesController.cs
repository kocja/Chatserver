using chatserver.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace chatserver.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ChatDbContext db;

        public MessagesController(ChatDbContext dbContext)
        {
            this.db = dbContext;
        }

        /// <summary>
        /// Liste aller Nachrichten
        /// </summary>
        /// <remarks>
        /// TODO: Ermöglichen, dass Nachrichten via Timestamp gefiltert werden können
        /// </remarks>
        [HttpGet]
        public IEnumerable<Message> Get()
        {
            List<Message> messages = new List<Message>();
            foreach (Message messageObj in this.db.Messages.OrderByDescending(p => p.timestamp))
            {
                messageObj.timestamp = new DateTime(messageObj.timestamp.Ticks, System.DateTimeKind.Utc);
                messages.Add(messageObj);
            }
            return messages;
        }

        /// <summary>
        /// Holt eine einzelne Nachricht via id
        /// </summary>
        /// <remarks>
        /// Bei einem Fehler, wird der Status Code 400 mit einer Fehlermeldung zurückgegeben
        /// </remarks>
        [HttpGet("{id}")]
        public JsonResult Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ChatResponse.Error("id should not be empty");
            }

            try
            {
                Message messageObj = this.db.Messages.Single(p => p.id == id);
                messageObj.timestamp = new DateTime(messageObj.timestamp.Ticks, System.DateTimeKind.Utc);
                return ChatResponse.Ok(messageObj);
            }
            catch
            {
                return ChatResponse.Error("message not found");
            }
        }

        /// <summary>
        /// Speichern einer Nachricht
        /// </summary>
        /// <remarks>
        /// Nur die Nachricht selber und die User-ID muss angegeben werden.
        /// Der Timestamp wird auf dem Server erstellt bzw. überschreibt alle übergebenen Werte.
        /// Konnte die Nachricht gespeichert werden, wird sie an alle verbundenen WebSocket Clients verschickt (serialisiertes JSON-Object mit action "message_added").
        /// </remarks>
        [HttpPost]
        public JsonResult Post([FromBody] Message message)
        {
            if (message == null)
            {
                return ChatResponse.Error("unable to parse message");
            }

            message.id = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(message.message))
            {
                return ChatResponse.Error("message should not be empty");
            }
            else
            {
                message.message = message.message.Trim();
            }

            User user;
            if (string.IsNullOrWhiteSpace(message.user_id))
            {
                return ChatResponse.Error("user_id should not be empty");
            }
            else
            {
                try
                {
                    user = this.db.Users.Single(p => p.id == message.user_id);
                }
                catch
                {
                    return ChatResponse.Error("user not found");
                }
            }

            if (user.status != UserStatus.online)
            {
                user.status = UserStatus.online;
                user.last_status_change = DateTime.UtcNow;
                ChatWebSocket.Send(new ChatWebSocketMessage()
                {
                    action = "user_updated",
                    data = user
                });
            }

            message.timestamp = DateTime.UtcNow;

            this.db.Add(message);
            try
            {
                this.db.SaveChanges();
            }
            catch (Exception e)
            {
                return ChatResponse.Error(e);
            }

            ChatWebSocket.Send(new ChatWebSocketMessage()
            {
                action = "message_added",
                data = message
            });

            return ChatResponse.Ok(new { id = message.id });
        }
    }
}
