using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using AuthService.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AuthService.Service;
using AuthService.Models;
using System;

namespace AuthService.Controllers
{
    [RequireHttps]
    public class RegisterController : Controller
    {
        private readonly UserDbContext _userDbContext;
        private readonly IMailService _mailService;

        public RegisterController(UserDbContext userDbContext, IMailService mailService)
        {
            _userDbContext = userDbContext;
            _mailService = mailService;
        }

        [HttpPost, Route("/register")]
        public async Task<IActionResult> Register(RegisterReq model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (_userDbContext.Users.Any(x => x.Email == model.Email))
                return StatusCode(StatusCodes.Status409Conflict);

            var confirmationToken = GenerateToken();
            _userDbContext.Users.Add(new User
            {
                Email = model.Email,
                Name = model.Name,
                PasswordHash = model.Password.Hash(),
                State = UserState.EmailConfirmation,
                ConirmationToken = confirmationToken
            });

            await _userDbContext.SaveChangesAsync();
            await _mailService.SendEmailConfirmation(model.Email, confirmationToken);

            return Ok();
        }

        [HttpPost, Route("/confirm")]
        public async Task<IActionResult> Confirm(EmailConfirmReq model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = _userDbContext.Users.FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
                return NotFound();

            if (user.State != UserState.EmailConfirmation)
                return StatusCode(StatusCodes.Status422UnprocessableEntity);

            if (string.Compare(user.ConirmationToken, model.Token, ignoreCase: true) != 0)
                return BadRequest("Invalid token");

            user.State = UserState.Active;
            user.ConirmationToken = null;
            await _userDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost, Route("/changePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordReq model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = _userDbContext.Users.FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
                return NotFound();

            if (!model.CurrentPassword.Verify(user.PasswordHash))
                return BadRequest();

            user.PasswordHash = model.NewPassword.Hash();
            await _userDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost, Route("/resendConfirmation")]
        public async Task<IActionResult> ResendConfirmation(ResendConfirmationReq model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = _userDbContext.Users.FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
                return NotFound();

            if (user.State != UserState.EmailConfirmation)
                return StatusCode(StatusCodes.Status422UnprocessableEntity);

            await _mailService.SendEmailConfirmation(model.Email, model.Email.Hash());
            return Ok();
        }

        [HttpPost, Route("/resetPassword")]
        public async Task<IActionResult> ResetPassword(ForgotPasswordReq model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = _userDbContext.Users.FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
                return NotFound();

            throw new System.Exception();
            return Ok();
        }

        private static string GenerateToken()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
