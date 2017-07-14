using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using AuthService.Service;
using AuthService.Extensions;
using Microsoft.AspNetCore.Http;
using AuthService.Models;

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

        [HttpPost, Route("/login")]
        public IActionResult Login(LoginReq model)
        {
            var user = _userDbContext.Users.FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
                return NotFound();

            if (user.State != UserState.Active)
                return StatusCode(StatusCodes.Status406NotAcceptable);

            if (!model.Password.Verify(user.PasswordHash))
                return BadRequest();

            var jwt = _jwtGenerator.GenerateJwt(user);

            return Ok(jwt);
        }
    }
}
