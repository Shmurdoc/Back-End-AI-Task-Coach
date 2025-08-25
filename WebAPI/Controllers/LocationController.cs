using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LocationController : ControllerBase
{
    [HttpGet("ip")]
    public IActionResult GetIp() => Ok(new { success = true, ip = HttpContext.Connection.RemoteIpAddress?.ToString() });
}
