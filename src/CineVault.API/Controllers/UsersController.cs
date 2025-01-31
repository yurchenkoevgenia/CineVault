namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;

    public UsersController(CineVaultDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        _logger.Information("Executing GetUsers method.");

        var users = await _dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        _logger.Information("Executing GetUserById method for user ID {UserId}.", id);

        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            _logger.Warning("User with ID {UserId} not found.", id);
            return NotFound();
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        _logger.Information("Executing CreateUser method for username {Username}.", request.Username);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        _logger.Information("Executing UpdateUser method for user ID {UserId}.", id);

        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            _logger.Warning("User with ID {UserId} not found for update.", id);
            return NotFound();
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.Password = request.Password;

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        _logger.Information("Executing DeleteUser method for user ID {UserId}.", id);

        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            _logger.Warning("User with ID {UserId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
