using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PWTransfer.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PWTransfer.Controllers
{
    public class FlushController : Controller
    {
        private readonly AccountDbContext _accountDbContext;
        private readonly ILogger<FlushController> _logger;

        public FlushController(AccountDbContext accountDbContext, ILoggerFactory loggerFactory)
        {
            _accountDbContext = accountDbContext;
            _logger = loggerFactory.CreateLogger<FlushController>();
        }

        [HttpPost]
        [Route("/flush")]
        public async Task<IActionResult> Flush()
        {
            _logger.LogInformation("Flushing...");

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

            _logger.LogInformation("Flush finished successfully.");

            return Json(report);
        }
    }
}
