using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthTestController : ControllerBase
    {
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult PublicPing()
        {
            return Ok(new { message = "public-ok" });
        }

        [HttpGet("private")]
        [Authorize]
        public IActionResult PrivatePing()
        {
            var userName = User?.Identity?.Name ?? "anonymous";
            return Ok(new { message = "private-ok", user = userName });
        }
    }
}
