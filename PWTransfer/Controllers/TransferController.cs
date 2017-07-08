using Microsoft.AspNetCore.Mvc;
using Shared;
using PWTransfer.Data;
using PWTransfer.Service;
using System.Transactions;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace PWTransfer.Controllers
{
    public class TransferController : Controller
    {
        private readonly AccountDbContext _accountDbContext;
        private readonly IAuthService _authService;

        public TransferController(AccountDbContext accountDbContext, IAuthService authService)
        {
            _accountDbContext = accountDbContext;
            _authService = authService;
        }

        public async Task<IActionResult> Info()
        {
            var accountId = await _authService.GetUserId();

            var changes = (from account in _accountDbContext.Accounts
                           join change in _accountDbContext.LastAccountChanges
                           on account.Id equals change.AccountId
                           where account.Id == accountId
                           select new
                           {
                               account.Id,
                               account.Value,
                               Change = change.Value
                           }).AsEnumerable();

            var info = changes.Aggregate(new AccountInfo(), (accountInfo, change) =>
                        {
                            if (accountInfo.UserId == UserId.Unknown)
                            {
                                accountInfo.UserId = change.Id;
                                accountInfo.Value = change.Value;
                            }

                            accountInfo.Value += change.Change;

                            return accountInfo;
                        });

            return Json(info);
        }

        [HttpPost]
        public async Task<IActionResult> Send(TransferAction action)
        {
            var sender = await _authService.GetUserId();

            using (var scope = new TransactionScope(TransactionScopeOption.Required, DefaultTransactionOptions.Default))
            {
                var actionLog = _accountDbContext.ActionLogs.Add(new ActionLog
                {
                    Sender = sender,
                    Receiver = action.Receiver,
                    Amount = action.Amount,
                    DateUtc = DateTime.UtcNow                    
                });

                _accountDbContext.LastAccountChanges.Add(new AccountChange
                {
                    AccountId = sender,
                    Value = -action.Amount,
                    ActionLog = actionLog.Entity
                });

                _accountDbContext.LastAccountChanges.Add(new AccountChange
                {
                    AccountId = action.Receiver,
                    Value = action.Amount,
                    ActionLog = actionLog.Entity
                });

                await _accountDbContext.SaveChangesAsync();
                scope.Complete();

                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Flush()
        {
            var accounts = _accountDbContext.LastAccountChanges.Select(x => x.AccountId).Distinct().ToList();
            var report = accounts.ToDictionary(x => x, _ => 0);

            foreach (var accountId in accounts)
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, DefaultTransactionOptions.Default))
                {
                    var changes = _accountDbContext.LastAccountChanges.Where(x => x.AccountId == accountId).ToList();
                    var account = _accountDbContext.Accounts.First(x => x.Id == accountId);

                    changes.ForEach(change => account.Value += change.Value);

                    _accountDbContext.LastAccountChanges.RemoveRange(changes);

                    await _accountDbContext.SaveChangesAsync();
                    scope.Complete();

                    report[accountId] = changes.Count();
                }
            }

            return Json(report);
        }
    }
}
