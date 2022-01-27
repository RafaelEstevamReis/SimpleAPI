using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Simple.TestWebApi.Controllers
{
    [Authorize]
    [ApiController]
    public class Account : ControllerBase
    {

        [HttpGet]
        [Route("WhoAmI")]
        public ActionResult WhoAmI() => Ok(new { User = User.Identity.Name });

    }
}
