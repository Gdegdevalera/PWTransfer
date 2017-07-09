using AccountService.Models;
using System;

namespace AccountService.Data
{
    public class Account
    {
        public UserId Id { get; set; }

        public Decimal Value { get; set; }
    }
}
