using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Mailer.Controllers
{
    public class EMailController : Controller
    {
        public IActionResult Send(Notification notification)
        {
            return StatusCode(200);
        }
    }
}
