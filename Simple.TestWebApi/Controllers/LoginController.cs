using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Simple.TestWebApi.Controllers
{
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("authenticate")]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody] Auth.Login model)
        {
            var user = Auth.User.Get(model.Username, model.Password, model.Mock_ReturnSuccess);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = Auth.Token.GenerateToken(user);
            return Ok(new
            {
                user,
                token
            });
        }
    }
}
