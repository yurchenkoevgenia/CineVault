using Asp.Versioning;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
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
    [MapToApiVersion(1)]
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
    [MapToApiVersion(1)]
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
    [MapToApiVersion(1)]
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
    [MapToApiVersion(1)]
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
    [MapToApiVersion(1)]
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

    [HttpGet]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsersUnified()
    {
        _logger.Information("Executing GetUsers method with unified response.");

        var users = await _dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();

        var response = ApiResponse<List<UserResponse>>.Success(users);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserByIdUnified(int id)
    {
        _logger.Information("Executing GetUserById method with unified response for user ID {UserId}.", id);

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

        var apiResponse = ApiResponse<UserResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUserUnified(ApiRequest<UserRequest> request)
    {
        _logger.Information("Executing CreateUser method with unified request for username {Username}.", request.Data.Username);

        var user = new User
        {
            Username = request.Data.Username,
            Email = request.Data.Email,
            Password = request.Data.Password
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        var apiResponse = ApiResponse<UserResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUserUnified(int id, ApiRequest<UserRequest> request)
    {
        _logger.Information("Executing UpdateUser method with unified request for user ID {UserId}.", id);

        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            _logger.Warning("User with ID {UserId} not found for update.", id);
            return NotFound();
        }

        user.Username = request.Data.Username;
        user.Email = request.Data.Email;
        user.Password = request.Data.Password;

        await _dbContext.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };

        var apiResponse = ApiResponse<UserResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUserUnified(int id)
    {
        _logger.Information("Executing DeleteUser method with unified response for user ID {UserId}.", id);

        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            _logger.Warning("User with ID {UserId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        var apiResponse = ApiResponse<string>.Success("User deleted successfully");
        return Ok(apiResponse);
    }
}
