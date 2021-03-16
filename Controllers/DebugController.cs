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
    public class DebugController : ControllerBase
    {
        private readonly ChatDbContext db;

        public DebugController(ChatDbContext dbContext)
        {
            this.db = dbContext;
        }

        /// <summary>
        /// Löscht eine einzelne Nachricht
        /// </summary>
        [HttpDelete("messages/{id}")]
        public ChatResponse Delete(string id)
        {
            using (ChatDbContext db = new ChatDbContext())
            {
                Message message;
                try
                {
                    message = db.Messages.Single(p => p.id == id);
                }
                catch
                {
                    return ChatResponse.Error("message not found");
                }
                db.Messages.Remove(message);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return ChatResponse.Error(e);
                }

                ChatWebSocket.Send(new ChatWebSocketMessage()
                {
                    action = "message_deleted",
                    data = new Message()
                    {
                        id = id
                    }
                });

                return ChatResponse.Ok(new { message = "message successfully deleted" });
            }
        }
    }
}
