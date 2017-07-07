using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace PWTransfer.Controllers
{
    public class TransferController : Controller
    {
        public TransferController()
        {

        }

        [HttpPost]
        public IActionResult Send(TransferAction action)
        {
            return StatusCode(400);
        }
    }
}
