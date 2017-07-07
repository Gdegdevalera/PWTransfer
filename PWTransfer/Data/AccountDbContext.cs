using Microsoft.EntityFrameworkCore;

namespace PWTransfer.Data
{
    public class AccountDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Action> LastActions { get; set; }

        public DbSet<ActionLog> ActionLog { get; set; }
    }
}
