using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using AuthService.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthService.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserDbContext _userDbContext;

        public RegisterController(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register(Models.RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var existUser = _userDbContext.Users.FirstOrDefault(x => x.Email == model.Email);

            if (existUser != null)
                return StatusCode(StatusCodes.Status409Conflict);

            _userDbContext.Users.Add(new User
            {
                Email = model.Email,
                Name = model.Name,
                PasswordHash = model.Password.Hash(),
                State = UserState.Active
            });

            await _userDbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
