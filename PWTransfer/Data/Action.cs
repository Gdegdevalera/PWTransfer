using Shared;
using System;

namespace PWTransfer.Data
{
    public class Action
    {
        public long Id { get; set; }

        public UserId UserId { get; set; }

        public Decimal Value { get; set; }
        
        public long ActionLogId { get; set; }
    }
}
