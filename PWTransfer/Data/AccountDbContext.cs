using Microsoft.EntityFrameworkCore;

namespace PWTransfer.Data
{
    public class AccountDbContext : DbContext
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
            : base(options)
        { }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<AccountChange> LastAccountChanges { get; set; }

        public DbSet<ActionLog> ActionLogs { get; set; }
    }
}
