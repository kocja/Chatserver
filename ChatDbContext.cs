using Microsoft.EntityFrameworkCore;

namespace chatserver
{
    public class ChatDbContext : DbContext
    {
        /*
        Create DB:
        TERM=xterm dotnet ef migrations add InitialCreate
        TERM=xterm dotnet ef database update
         */
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=chat.db");
        }

        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Message> Messages { get; set; }
    }
}