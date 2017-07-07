using System;
using System.Transactions;

namespace PWTransfer.Data
{
    public static class DefaultTransactionOptions
    {
        public static TransactionOptions Default
        {
            get
            {
                return new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = TimeSpan.Zero
                };
            }
        }
    }
}
