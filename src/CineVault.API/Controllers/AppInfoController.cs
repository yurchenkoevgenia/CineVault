using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public AppInfoController(IWebHostEnvironment environment, ILogger logger)
    {
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("environment")]
    public IActionResult GetEnvironment()
    {
        _logger.Information("Executing Information logging");

        _logger.Warning("Executing Warning logging");

        _logger.Error("Executing Error logging");

        return Ok(new { Environment = _environment.EnvironmentName });
    }
}
