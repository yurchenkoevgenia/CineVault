﻿using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UsersController(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        _logger.Information("Executing GetUsers method.");
        var users = await _dbContext.Users.ToListAsync();
        return Ok(_mapper.Map<List<UserResponse>>(users));
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
        return Ok(_mapper.Map<UserResponse>(user));
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        _logger.Information("Executing CreateUser method for username {Username}.", request.Username);
        var user = _mapper.Map<User>(request);
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
        _mapper.Map(request, user);
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
        var users = await _dbContext.Users.ToListAsync();
        return Ok(ApiResponse<List<UserResponse>>.Success(_mapper.Map<List<UserResponse>>(users)));
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
        return Ok(ApiResponse<UserResponse>.Success(_mapper.Map<UserResponse>(user)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUserUnified(ApiRequest<UserRequest> request)
    {
        _logger.Information("Executing CreateUser method with unified request for username {Username}.", request.Data.Username);
        var user = _mapper.Map<User>(request.Data);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return Ok(ApiResponse<UserResponse>.Success(_mapper.Map<UserResponse>(user)));
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
        _mapper.Map(request.Data, user);
        await _dbContext.SaveChangesAsync();
        return Ok(ApiResponse<UserResponse>.Success(_mapper.Map<UserResponse>(user)));
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
        return Ok(ApiResponse<string>.Success("User deleted successfully"));
    }
}
