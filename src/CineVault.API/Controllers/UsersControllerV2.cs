using Asp.Versioning;
using Microsoft.Extensions.Logging;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class UsersControllerV2 : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UsersControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._mapper = mapper;
    }

    // TODO 2 Пошук користувачів
    [HttpGet("search")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> SearchUsers(
        [FromQuery] string? username,
        [FromQuery] string? email,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] string? sortBy = "username",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.Information("Executing SearchUsers with filters.");

        var query = _dbContext.Users.AsQueryable();

        // Фільтрація
        if (!string.IsNullOrEmpty(username))
            query = query.Where(u => u.Username.Contains(username));

        if (!string.IsNullOrEmpty(email))
            query = query.Where(u => u.Email.Contains(email));

        if (createdFrom.HasValue)
            query = query.Where(u => u.CreatedAt >= createdFrom.Value);

        if (createdTo.HasValue)
            query = query.Where(u => u.CreatedAt <= createdTo.Value);

        // Сортування
        query = sortBy?.ToLower() switch
        {
            "email" => query.OrderBy(u => u.Email),
            "createdat" => query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.Username)
        };

        // Пагінація
        var totalUsers = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var response = ApiResponse.Success(
        _mapper.Map<List<UserResponse>>(users),
        $"Total Users: {totalUsers}");

        return Ok(response);
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsersUnified(ApiRequest request)
    {
        this._logger.Information("Executing GetUsers method with unified response.");
        var users = await this._dbContext.Users.ToListAsync();
        return this.Ok(ApiResponse.Success(this._mapper.Map<List<UserResponse>>(users)));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserByIdUnified(ApiRequest request, int id)
    {
        this._logger.Information(
            "Executing GetUserById method with unified response for user ID {UserId}.", id);
        var user = await this._dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this._logger.Warning("User with ID {UserId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(this._mapper.Map<UserResponse>(user)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUserUnified(ApiRequest<UserRequest> request)
    {
        this._logger.Information(
            "Executing CreateUser method with unified request for username {Username}.",
            request.Data.Username);

        var user = this._mapper.Map<User>(request.Data);

        this._dbContext.Users.Add(user);
        await this._dbContext.SaveChangesAsync();

        // TODO 8 Доробити всі методи по створенню
        return this.Ok(ApiResponse.Success(new { UserId = user.Id }));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUserUnified(int id, ApiRequest<UserRequest> request)
    {
        this._logger.Information(
            "Executing UpdateUser method with unified request for user ID {UserId}.", id);
        var user = await this._dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this._logger.Warning("User with ID {UserId} not found for update.", id);
            return this.NotFound();
        }

        this._mapper.Map(request.Data, user);
        await this._dbContext.SaveChangesAsync();
        return this.Ok(ApiResponse.Success(this._mapper.Map<UserResponse>(user)));
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUserUnified(ApiRequest request, int id)
    {
        this._logger.Information(
            "Executing DeleteUser method with unified response for user ID {UserId}.", id);
        var user = await this._dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this._logger.Warning("User with ID {UserId} not found for deletion.", id);
            return this.NotFound();
        }

        this._dbContext.Users.Remove(user);
        await this._dbContext.SaveChangesAsync();
        return this.Ok(ApiResponse.Success("User deleted successfully", 200));
    }
}