using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            success = true, 
            message = "AI Task Coach API is working!", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    [HttpGet("swagger-test")]
    public IActionResult SwaggerTest()
    {
        return Ok(new { 
            swagger_working = true, 
            api_endpoints_available = true,
            openapi_version = "3.0.1"
        });
    }
}