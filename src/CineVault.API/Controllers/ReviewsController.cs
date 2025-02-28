using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public ReviewsController(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        this._logger.Information("Executing GetReviews method.");

        var reviews = await this._dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();

        return this.Ok(this._mapper.Map<List<ReviewResponse>>(reviews));
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        this._logger.Information("Executing GetReviewById method for ID {ReviewId}.", id);

        var review = await this._dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            this._logger.Warning("Review with ID {ReviewId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(this._mapper.Map<ReviewResponse>(review));
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        this._logger.Information(
            "Executing CreateReview method for movie ID {MovieId} and user ID {UserId}.",
            request.MovieId, request.UserId);

        var review = this._mapper.Map<Review>(request);
        this._dbContext.Reviews.Add(review);
        await this._dbContext.SaveChangesAsync();

        return this.Created();
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        this._logger.Information("Executing UpdateReview method for review ID {ReviewId}.", id);

        var review = await this._dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this._logger.Warning("Review with ID {ReviewId} not found for update.", id);
            return this.NotFound();
        }

        this._mapper.Map(request, review);
        await this._dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteReview(int id)
    {
        this._logger.Information("Executing DeleteReview method for review ID {ReviewId}.", id);

        var review = await this._dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this._logger.Warning("Review with ID {ReviewId} not found for deletion.", id);
            return this.NotFound();
        }

        this._dbContext.Reviews.Remove(review);
        await this._dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetReviewsUnified(ApiRequest request)
    {
        this._logger.Information("Executing GetReviews method with unified response.");

        var reviews = await this._dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();

        return this.Ok(
            ApiResponse.Success(this._mapper.Map<List<ReviewResponse>>(reviews)));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewByIdUnified(ApiRequest request, int id)
    {
        this._logger.Information(
            "Executing GetReviewById method with unified response for ID {ReviewId}.", id);

        var review = await this._dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            this._logger.Warning("Review with ID {ReviewId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(this._mapper.Map<ReviewResponse>(review)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReviewUnified(ApiRequest<ReviewRequest> request)
    {
        this._logger.Information(
            "Executing CreateReview method with unified request for movie ID {MovieId} and user ID {UserId}.",
            request.Data.MovieId, request.Data.UserId);

        var review = this._mapper.Map<Review>(request.Data);
        this._dbContext.Reviews.Add(review);
        await this._dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success(this._mapper.Map<ReviewResponse>(review)));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReviewUnified(int id, ApiRequest<ReviewRequest> request)
    {
        this._logger.Information(
            "Executing UpdateReview method with unified request for review ID {ReviewId}.", id);

        var review = await this._dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this._logger.Warning("Review with ID {ReviewId} not found for update.", id);
            return this.NotFound();
        }

        this._mapper.Map(request.Data, review);
        await this._dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success(this._mapper.Map<ReviewResponse>(review)));
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteReviewUnified(ApiRequest request, int id)
    {
        this._logger.Information(
            "Executing DeleteReview method with unified response for review ID {ReviewId}.", id);

        var review = await this._dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this._logger.Warning("Review with ID {ReviewId} not found for deletion.", id);
            return this.NotFound();
        }

        this._dbContext.Reviews.Remove(review);
        await this._dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success("Review successfully deleted.", 200));
    }
}