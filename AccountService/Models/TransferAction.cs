using System;

namespace AccountService.Models
{
    public class TransferAction
    {
        public UserId Receiver { get; set; }

        public Decimal Amount { get; set; }
    }
}
