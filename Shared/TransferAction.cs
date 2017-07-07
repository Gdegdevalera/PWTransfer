using System;

namespace Shared
{
    public class TransferAction
    {
        public UserId Sender { get; set; }

        public UserId Receiver { get; set; }

        public Decimal Amount { get; set; }
    }
}
