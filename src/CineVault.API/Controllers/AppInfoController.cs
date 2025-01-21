using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public AppInfoController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment")]
    [MapToApiVersion(1)]
    public IActionResult GetEnvironmentV1()
    {
        return Ok(new { Environment = _environment.EnvironmentName });
    }

    [HttpGet("environment")]
    [MapToApiVersion(2)]
    public IActionResult GetEnvironmentV2()
    {
        return Ok(new { Environment = _environment.EnvironmentName, ApiVersion = "2.0" });
    }
}

