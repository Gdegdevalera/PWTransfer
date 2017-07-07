using Shared;
using System;

namespace PWTransfer.Data
{
    public class ActionLog
    {
        public long Id { get; set; }

        public UserId Sender { get; set; }

        public UserId Receiver { get; set; }

        public Decimal Amount { get; set; }

        public DateTime DateUtc { get; set; }
    }
}
