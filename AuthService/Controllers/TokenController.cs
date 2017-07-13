using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using AuthService.Service;
using AuthService.Extensions;
using Microsoft.AspNetCore.Http;

namespace AuthService.Controllers
{
    public class TokenController : Controller
    {
        private readonly UserDbContext _userDbContext;
        private readonly IJwtGenerator _jwtGenerator;

        public TokenController(UserDbContext userDbContext, IJwtGenerator jwtGenerator)
        {
            _userDbContext = userDbContext;
            _jwtGenerator = jwtGenerator;
        }

        [HttpPost, Route("/token")]
        public IActionResult Token()
        {
            var email = Request.Form["email"].Single();
            var password = Request.Form["password"].Single();

            var user = _userDbContext.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
                return NotFound();

            if (!password.Verify(user.PasswordHash))
                return BadRequest();

            if (user.State != UserState.Active)
                return StatusCode(StatusCodes.Status406NotAcceptable);

            var jwt = _jwtGenerator.GenerateJwt(user);

            return Ok(jwt);
        }
    }
}
