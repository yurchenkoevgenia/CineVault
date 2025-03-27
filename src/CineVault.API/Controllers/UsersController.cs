using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UsersController(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        this._logger.Information("Executing GetUsers method.");
        var users = await this._dbContext.Users.ToListAsync();
        return this.Ok(this._mapper.Map<List<UserResponse>>(users));
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        this._logger.Information("Executing GetUserById method for user ID {UserId}.", id);
        var user = await this._dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this._logger.Warning("User with ID {UserId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(this._mapper.Map<UserResponse>(user));
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        this._logger.Information("Executing CreateUser method for username {Username}.",
            request.Username);
        var user = this._mapper.Map<User>(request);
        this._dbContext.Users.Add(user);
        await this._dbContext.SaveChangesAsync();
        return this.Ok();
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        this._logger.Information("Executing UpdateUser method for user ID {UserId}.", id);
        var user = await this._dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this._logger.Warning("User with ID {UserId} not found for update.", id);
            return this.NotFound();
        }

        this._mapper.Map(request, user);
        await this._dbContext.SaveChangesAsync();
        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        this._logger.Information("Executing DeleteUser method for user ID {UserId}.", id);
        var user = await this._dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this._logger.Warning("User with ID {UserId} not found for deletion.", id);
            return this.NotFound();
        }

        this._dbContext.Users.Remove(user);
        await this._dbContext.SaveChangesAsync();
        return this.Ok();
    }
}