using chatserver.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace chatserver
{
    public class ChatUserTracking : IHostedService, IDisposable
    {
        private Timer _timer;
        private const int _checkInterval = 60;

        private readonly ILogger<ChatUserTracking> _logger;

        public ChatUserTracking(ILogger<ChatUserTracking> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("User Tracking Service started.");

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(_checkInterval), TimeSpan.FromSeconds(_checkInterval));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            List<User> updatedUsers = new List<User>();
            using (ChatDbContext db = new ChatDbContext())
            {
                DateTime awayLimit = DateTime.UtcNow.AddHours(-4);
                foreach (User user in db.Users.Where(p => p.status == UserStatus.away))
                {
                    DateTime usertimestamp = new DateTime(user.last_status_change.Ticks, System.DateTimeKind.Utc);
                    if (usertimestamp < awayLimit)
                    {
                        Console.WriteLine("User " + user.nickname + " is away since " + usertimestamp.ToLocalTime() + ", setting it as offline");
                        user.status = UserStatus.offline;
                        user.last_status_change = DateTime.UtcNow;
                        updatedUsers.Add(user);
                    }
                }

                DateTime onlineLimit = DateTime.UtcNow.AddMinutes(-30);
                foreach (User user in db.Users.Where(p => p.status == UserStatus.online))
                {
                    DateTime usertimestamp = new DateTime(user.last_status_change.Ticks, System.DateTimeKind.Utc);
                    if (usertimestamp < onlineLimit)
                    {
                        Console.WriteLine("User " + user.nickname + " is online since " + usertimestamp.ToLocalTime() + ", setting it as away");
                        user.status = UserStatus.away;
                        user.last_status_change = DateTime.UtcNow;
                        updatedUsers.Add(user);
                    }
                }

                if (updatedUsers.Any())
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to store updated users to db: " + e.Message);
                    }
                }
            }

            foreach (User user in updatedUsers)
            {
                ChatWebSocket.Send(new ChatWebSocketMessage()
                {
                    action = "user_updated",
                    data = user
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}