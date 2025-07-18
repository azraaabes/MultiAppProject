using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Webapiproje.Controllers { 

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestTokenController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name ?? "Bilinmiyor";
            return Ok($"Hoş geldin, {username}!");
        }
    }
}
