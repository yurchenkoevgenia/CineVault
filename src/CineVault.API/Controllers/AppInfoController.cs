using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public AppInfoController(IWebHostEnvironment environment, ILogger logger)
    {
        this._environment = environment;
        this._logger = logger;
    }

    [HttpGet("environment")]
    public IActionResult GetEnvironment()
    {
        this._logger.Information("Executing Information logging");

        this._logger.Warning("Executing Warning logging");

        this._logger.Error("Executing Error logging");

        return this.Ok(new { Environment = this._environment.EnvironmentName });
    }
}
