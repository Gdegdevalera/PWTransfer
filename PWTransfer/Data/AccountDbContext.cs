using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace PWTransfer.Data
{
    public class AccountDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DbSet<AccountChange> LastAccountChanges { get; set; }

        public DbSet<ActionLog> ActionLogs { get; set; }
    }
}
