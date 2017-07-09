using AccountService.Models;
using System;

namespace AccountService.Data
{
    public class AccountChange
    {
        public long Id { get; set; }

        public UserId AccountId { get; set; }

        public Decimal Value { get; set; }

        public long ActionLogId { get; set; }

        public virtual ActionLog ActionLog { get; set; }
    }
}
