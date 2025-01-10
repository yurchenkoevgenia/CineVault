using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

//Завдання e: Контролер AppInfoController

[ApiController]
[Route("api/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public AppInfoController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment")]
    public IActionResult GetEnvironment()
    {
        return Ok(new { Environment = _environment.EnvironmentName });
    }
}

