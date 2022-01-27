using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Simple.TestWebApi.Controllers
{
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody] Auth.Login model)
        {
            var user = Auth.User.Get(model.Username, model.Password, model.Mock_ReturnSuccess);

            if (user == null)
                return NotFound(new { message = "Invalid credentials" });

            var token = Auth.Token.GenerateToken(user);
            return Ok(new
            {
                user,
                token
            });
        }
    }
}
