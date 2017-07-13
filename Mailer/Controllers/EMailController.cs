using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System.Threading.Tasks;

namespace Mailer.Controllers
{
    public class EMailController : Controller
    {
        private readonly IEmailService _emailService;

        public EMailController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        
        [HttpPost, Route("/send")]
        public async Task<IActionResult> Send(Notification model)
        {
            await _emailService.SendAsync(model.EMail, model.Subject, model.Body);

            return Ok();
        }
    }

    public class Notification
    {
        public string EMail { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
