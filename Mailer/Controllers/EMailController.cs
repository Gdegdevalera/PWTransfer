using Microsoft.AspNetCore.Mvc;

namespace Mailer.Controllers
{
    public class EMailController : Controller
    {
        public IActionResult Send(Notification notification)
        {
            return Ok();
        }
    }

    public class Notification
    {

    }
}
