using Microsoft.AspNetCore.Mvc;
using AccountService.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AccountService.Models;

namespace AccountService.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AccountDbContext _accountDbContext;
        private readonly ILogger<AccountController> _logger;

        private const decimal InitialValue = 500;

        public AccountController(AccountDbContext accountDbContext, ILoggerFactory loggerFactory)
        {
            _accountDbContext = accountDbContext;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpPost, Route("/create")]
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
            return Ok(accountId);
        }

        [HttpGet, Route("/info")]
        public IActionResult Info()
        {
            var accountId = GetUserId();
            var accountInfo = GetAccountInfo(accountId);

            if (accountInfo == null)
                return NotFound();

            return Json(accountInfo);
        }

        [HttpPost, Route("/send")]
        public async Task<IActionResult> Send(TransferAction model)
        {
            var sender = GetUserId();

            if (Invalid(model.Amount, sender))
                return BadRequest();

            var receiverExists = _accountDbContext.Accounts.Any(x => x.Id == model.Receiver);

            if (model.Receiver == sender || !receiverExists)
                return BadRequest();

            using (var transaction = await _accountDbContext.Database.BeginTransactionAsync())
            {
                var actionLog = _accountDbContext.ActionLogs.Add(new ActionLog
                {
                    Sender = sender,
                    Receiver = model.Receiver,
                    Amount = model.Amount,
                    DateUtc = DateTime.UtcNow
                });

                _accountDbContext.LastAccountChanges.Add(new AccountChange
                {
                    AccountId = sender,
                    Value = -model.Amount,
                    ActionLog = actionLog.Entity
                });

                _accountDbContext.LastAccountChanges.Add(new AccountChange
                {
                    AccountId = model.Receiver,
                    Value = model.Amount,
                    ActionLog = actionLog.Entity
                });

                await _accountDbContext.SaveChangesAsync();
                transaction.Commit();

                return Ok();
            }
        }

        private AccountInfo GetAccountInfo(UserId accountId)
        {
            var accountInfo = _accountDbContext.Accounts
                .Select(x => new AccountInfo { UserId = x.Id, Value = x.Value })
                .FirstOrDefault(x => x.UserId == accountId);

            if (accountInfo == null)
                return null;

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

                return info;
            }
            else
            {
                return accountInfo;
            }
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

        private bool Invalid(decimal amount, UserId sender)
        {
            if (amount <= 0)
                return true;

            if (amount != Math.Round(amount, 2, MidpointRounding.AwayFromZero))
                return true;

            var value = GetAccountInfo(sender).Value;
            return value - amount < 0;
        }
    }
}
