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
    public class UsersController : ControllerBase
    {
        private readonly ChatDbContext db;

        public UsersController(ChatDbContext dbContext)
        {
            this.db = dbContext;
        }

        /// <summary>
        /// Liste aller Benutzer
        /// </summary>
        /// <remarks>
        /// TODO: Filtern nach Status oder last_status_change oder ...
        /// </remarks>
        [HttpGet]
        public IEnumerable<User> Get()
        {
            List<User> users = new List<User>();
            foreach (User userObj in this.db.Users.OrderBy(p => p.nickname))
            {
                userObj.created = new DateTime(userObj.created.Ticks, System.DateTimeKind.Utc);
                userObj.updated = new DateTime(userObj.updated.Ticks, System.DateTimeKind.Utc);
                userObj.last_status_change = new DateTime(userObj.last_status_change.Ticks, System.DateTimeKind.Utc);
                users.Add(userObj);
            }

            return users;
        }

        /// <summary>
        /// Holt einen einzelnen Benutzer
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
                User userObj = this.db.Users.Single(p => p.id == id);
                userObj.created = new DateTime(userObj.created.Ticks, System.DateTimeKind.Utc);
                userObj.updated = new DateTime(userObj.updated.Ticks, System.DateTimeKind.Utc);
                userObj.last_status_change = new DateTime(userObj.last_status_change.Ticks, System.DateTimeKind.Utc);
                return ChatResponse.Ok(userObj);
            }
            catch
            {
                return ChatResponse.Error("user not found");
            }
        }

        /// <summary>
        /// Legt einen neuen Benutzer an
        /// </summary>
        /// <remarks>
        /// Nur der Nickname muss angegeben werden. Ohne status Angabe wird "offline" verwendet.
        /// Die 3 Timestamp Felder werden vom Server gesetzt (created, updated, last_status_change).
        /// Konnte der Benutzer gespeichert werden, wird er an alle verbundenen WebSocket Clients verschickt (serialisiertes JSON-Object mit action "user_added").
        /// </remarks>
        [HttpPost]
        public JsonResult Post([FromBody] User user)
        {
            if (user == null)
            {
                return ChatResponse.Error("unable to parse user");
            }

            user.id = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(user.nickname))
            {
                return ChatResponse.Error("nickname should not be empty");
            }

            user.ParseInput();

            if (this.db.Users.Count(p => p.nickname == user.nickname) > 0)
            {
                return ChatResponse.Error("this nickname is already in use");
            }

            user.created = DateTime.UtcNow;
            user.updated = user.created;
            user.last_status_change = user.created;

            this.db.Add(user);
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
                action = "user_added",
                data = user
            });

            return ChatResponse.Ok(new { id = user.id });
        }

        /// <summary>
        /// Die Angaben eines Benutzers verändern
        /// </summary>
        /// <remarks>
        /// Damit können die Angaben eines Benutzers aktualisiert werden, konkret der Nickname, Status, Avatar und die Description.
        /// Wird ein Feld ausgelassen (oder hat einen Null-Wert) dann wird das entsprechende Feld nicht aktualisiert.
        /// Konnte der Benutzer aktualisiert werden, wird er an alle verbundenen WebSocket Clients verschickt (serialisiertes JSON-Object mit action "user_updated").
        /// </remarks>
        [HttpPut("{id}")]
        public ChatResponse Put(string id, [FromBody] UserUpdate user)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ChatResponse.Error("id should not be empty");
            }

            if (user == null)
            {
                return ChatResponse.Error("unable to parse user");
            }

            User userFromDB;
            try
            {
                userFromDB = this.db.Users.Single(p => p.id == id);
            }
            catch
            {
                return ChatResponse.Error("user not found");
            }

            user.ParseInput();

            if (user.nickname != null && user.nickname != userFromDB.nickname)
            {
                if (this.db.Users.Count(p => p.nickname == user.nickname) > 0)
                {
                    return ChatResponse.Error("this nickname is already in use");
                }
                else
                {
                    userFromDB.nickname = user.nickname;
                }
            }

            if (user.avatar != null && user.avatar != userFromDB.avatar)
            {
                userFromDB.avatar = user.avatar;
            }

            if (user.description != null && user.description != userFromDB.description)
            {
                userFromDB.description = user.description;
            }

            if (user.status != null && user.status != userFromDB.status)
            {
                userFromDB.status = (UserStatus)user.status;
                userFromDB.last_status_change = DateTime.UtcNow;
            }

            userFromDB.updated = DateTime.UtcNow;

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
                action = "user_updated",
                data = userFromDB
            });

            return ChatResponse.Ok(userFromDB);
        }

        /// <summary>
        /// Löscht einen Benutzer
        /// </summary>
        /// <remarks>
        /// Damit werden auch alle von diesem Benutzer erstellten Nachrichten gelöscht!
        /// Konnte der Benutzer gelöscht werden, wird er an alle verbundenen WebSocket Clients verschickt (serialisiertes JSON-Object mit action "user_deleted").
        /// </remarks>
        [HttpDelete("{id}")]
        public ChatResponse Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return ChatResponse.Error("id should not be empty");
            }

            using (ChatDbContext db = new ChatDbContext())
            {
                User user;
                try
                {
                    user = db.Users.Single(p => p.id == id);
                }
                catch
                {
                    return ChatResponse.Error("user not found");
                }
                db.Messages.RemoveRange(db.Messages.Where(p => p.user_id == user.id));
                db.Users.Remove(user);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return ChatResponse.Error(e);
                }
            }

            ChatWebSocket.Send(new ChatWebSocketMessage()
            {
                action = "user_deleted",
                data = new User()
                {
                    id = id
                }
            });

            return ChatResponse.Ok(new { message = "user successfully deleted" });
        }
    }
}
