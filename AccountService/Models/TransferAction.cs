using System;

namespace AccountService.Models
{
    public class TransferAction
    {
        public UserId Receiver { get; set; }

        public Decimal Amount { get; set; }
    }

    public enum UserId : long
    {
        Unknown = 0
    }

    public class AccountInfo
    {
        public UserId UserId { get; set; }

        public Decimal Value { get; set; }
    }
}
