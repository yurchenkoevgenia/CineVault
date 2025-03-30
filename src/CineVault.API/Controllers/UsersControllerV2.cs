using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class UsersControllerV2 : ControllerBase
{
    // TODO 13 Виконати оптимізацію роботи з EFCore
    private static readonly Func<CineVaultDbContext, int, Task<User?>> GetUserByIdQuery =
        EF.CompileAsyncQuery((CineVaultDbContext context, int id) =>
            context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id));

    private readonly CineVaultDbContext dbContext;
    private readonly ILogger logger;
    private readonly IMapper mapper;

    public UsersControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    // TODO 13 Виконати оптимізацію роботи з EFCore
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
        this.logger.Information("Executing SearchUsers with filters.");

        var query = this.dbContext.Users.AsQueryable();

        if (!string.IsNullOrEmpty(username))
        {
            query = query.Where(u => u.Username.Contains(username));
        }

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(u => u.Email.Contains(email));
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= createdTo.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "email" => query.OrderBy(u => u.Email),
            "createdat" => query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.Username)
        };

        int totalUsers = await query.CountAsync();
        var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var response = ApiResponse.Success(this.mapper.Map<List<UserResponse>>(users),
            $"Total Users: {totalUsers}");

        return this.Ok(response);
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsersUnified(
        ApiRequest request)
    {
        this.logger.Information("Executing GetUsers method with unified response.");
        var users = await this.dbContext.Users.ToListAsync();
        return this.Ok(ApiResponse.Success(this.mapper.Map<List<UserResponse>>(users)));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserByIdUnified(
        ApiRequest request, int id)
    {
        this.logger.Information(
            "Executing GetUserById method with unified response for user ID {UserId}.", id);
        var user = await GetUserByIdQuery(this.dbContext, id);
        if (user is null)
        {
            this.logger.Warning("User with ID {UserId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(this.mapper.Map<UserResponse>(user)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUserUnified(
        ApiRequest<UserRequest> request)
    {
        this.logger.Information(
            "Executing CreateUser method with unified request for username {Username}.",
            request.Data.Username);

        var user = this.mapper.Map<User>(request.Data);

        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success(new { UserId = user.Id }));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUserUnified(int id,
        ApiRequest<UserRequest> request)
    {
        this.logger.Information(
            "Executing UpdateUser method with unified request for user ID {UserId}.", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.Warning("User with ID {UserId} not found for update.", id);
            return this.NotFound();
        }

        this.mapper.Map(request.Data, user);
        await this.dbContext.SaveChangesAsync();
        return this.Ok(ApiResponse.Success(this.mapper.Map<UserResponse>(user)));
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUserUnified(ApiRequest request,
        int id)
    {
        this.logger.Information(
            "Executing DeleteUser method with unified response for user ID {UserId}.", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.Warning("User with ID {UserId} not found for deletion.", id);
            return this.NotFound();
        }

        // TODO 10 Реалізувати Soft Delete
        user.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();
        return this.Ok(ApiResponse.Success("User deleted successfully", 200));
    }

    // TODO 9 Додати такі нові методи в API
    [HttpPost("get/stats/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<UserStatsResponse>>> GetUserStatsUnified(
        ApiRequest request, int id)
    {
        this.logger.Information("Executing GetUserStats for user ID {UserId}.", id);

        var user = await this.dbContext.Users
            .Include(u => u.Reviews)
            .ThenInclude(r => r.Movie)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            this.logger.Warning("User with ID {UserId} not found.", id);
            return this.NotFound();
        }

        var stats = new UserStatsResponse
        {
            TotalReviews = user.Reviews.Count,
            AverageRating = user.Reviews.Any() ? user.Reviews.Average(r => r.Rating) : 0,
            LastActivity = user.Reviews.Any() ? user.Reviews.Max(r => r.CreatedAt) : null,
            GenreStats = user.Reviews
                .GroupBy(r => r.Movie.Genre)
                .Select(g => new UserStatsResponse.GenreStat
                {
                    Genre = g.Key,
                    TotalReviews = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .OrderByDescending(g => g.TotalReviews)
                .ToList()
        };

        return this.Ok(ApiResponse.Success(stats));
    }

    // TODO 10 Реалізувати Soft Delete
    [HttpPost("restore/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> RestoreUserUnified(ApiRequest request,
        int id)
    {
        this.logger.Information(
            "Executing RestoreUser method with unified response for user ID {UserId}.", id);

        var user = await this.dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            this.logger.Warning("User with ID {UserId} not found for restoration.", id);
            return this.NotFound();
        }

        if (!user.IsDeleted)
        {
            this.logger.Warning("User with ID {UserId} is not marked as deleted.", id);
            return this.BadRequest(ApiResponse.Failure("User is not deleted"));
        }

        user.IsDeleted = false;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success("User restored successfully", 200));
    }
}