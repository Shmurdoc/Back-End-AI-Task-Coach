using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OCRController : ControllerBase
{
    private readonly IOCRService _ocr;
    public OCRController(IOCRService ocr) => _ocr = ocr;

    [HttpPost("extract")]
    public async Task<IActionResult> Extract([FromBody] OCRRequest req)
    {
        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(req.ImageBase64);
        }
        catch (FormatException)
        {
            return BadRequest(new { success = false, error = "Invalid base64 string." });
        }
        var text = await _ocr.ExtractTextAsync(imageBytes);
        return Ok(new { success = true, data = text });
    }

    public record OCRRequest(string ImageBase64);
}
