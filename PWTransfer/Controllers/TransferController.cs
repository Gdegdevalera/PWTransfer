using Microsoft.AspNetCore.Mvc;
using Shared;
using PWTransfer.Data;
using System.Transactions;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PWTransfer.Controllers
{
    [Authorize]
    public class TransferController : Controller
    {
        private readonly AccountDbContext _accountDbContext;
        private readonly ILogger<TransferController> _logger;

        private const decimal InitialValue = 500;

        public TransferController(AccountDbContext accountDbContext, ILoggerFactory loggerFactory)
        {
            _accountDbContext = accountDbContext;
            _logger = loggerFactory.CreateLogger<TransferController>();
        }

        [Route("/create")]
        public async Task<IActionResult> Create()
        {
            var accountId = GetUserId();

            if (_accountDbContext.Accounts.Any(x => x.Id == accountId))
                return StatusCode(StatusCodes.Status409Conflict);

            _accountDbContext.Accounts.Add(new Account
            {
                Id = accountId,
                Value = InitialValue
            });

            await _accountDbContext.SaveChangesAsync();
            return Ok();
        }

        [Route("/info")]
        public IActionResult Info()
        {
            var accountId = GetUserId();

            var accountInfo = _accountDbContext.Accounts
                .Select(x => new AccountInfo { UserId = x.Id, Value = x.Value })
                .FirstOrDefault(x => x.UserId == accountId);

            if (accountInfo == null)
                return NotFound();

            // It's important to fetch changes with account value by single query
            // Because flush operation can be executed between different queries
            var changes = (from account in _accountDbContext.Accounts
                           join change in _accountDbContext.LastAccountChanges
                                on account.Id equals change.AccountId
                           where account.Id == accountId
                           select new
                           {
                               account.Id,
                               account.Value,
                               Change = change.Value
                           }).ToList();

            if (changes.Any())
            {
                var info = changes.Aggregate(new AccountInfo(), (accumulator, change) =>
                            {
                                if (accumulator.UserId == UserId.Unknown)
                                {
                                    accumulator.UserId = change.Id;
                                    accumulator.Value = change.Value;
                                }

                                accumulator.Value += change.Change;

                                return accumulator;
                            });

                return Json(info);
            }
            else
            {
                return Json(accountInfo);
            }
        }

        [Route("/send")]
        [HttpPost]
        public async Task<IActionResult> Send(TransferAction action)
        {
            var sender = GetUserId();

            using (var transaction = await _accountDbContext.Database.BeginTransactionAsync())
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
                transaction.Commit();

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
                using (var transaction = await _accountDbContext.Database.BeginTransactionAsync())
                {
                    var changes = _accountDbContext.LastAccountChanges.Where(x => x.AccountId == accountId).ToList();
                    var account = _accountDbContext.Accounts.First(x => x.Id == accountId);

                    changes.ForEach(change => account.Value += change.Value);

                    _accountDbContext.LastAccountChanges.RemoveRange(changes);

                    await _accountDbContext.SaveChangesAsync();
                    transaction.Commit();

                    report[accountId] = changes.Count();
                }
            }

            return Json(report);
        }

        private UserId GetUserId()
        {
            try
            {
                return (UserId)long.Parse(User.Claims.Single(x => x.Type == "userId").Value);
            }
            catch 
            {
                _logger.LogError("Invalid JWT found. Can't extract UserId");
                throw;
            }
        }
    }
}
