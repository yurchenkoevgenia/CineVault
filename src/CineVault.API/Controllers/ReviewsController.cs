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
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        _logger.Information("Executing GetReviews method.");

        var reviews = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();

        return Ok(_mapper.Map<List<ReviewResponse>>(reviews));
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        _logger.Information("Executing GetReviewById method for ID {ReviewId}.", id);

        var review = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found.", id);
            return NotFound();
        }

        return Ok(_mapper.Map<ReviewResponse>(review));
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        _logger.Information("Executing CreateReview method for movie ID {MovieId} and user ID {UserId}.", request.MovieId, request.UserId);

        var review = _mapper.Map<Review>(request);
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        return Created();
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        _logger.Information("Executing UpdateReview method for review ID {ReviewId}.", id);

        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found for update.", id);
            return NotFound();
        }

        _mapper.Map(request, review);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteReview(int id)
    {
        _logger.Information("Executing DeleteReview method for review ID {ReviewId}.", id);

        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetReviewsUnified()
    {
        _logger.Information("Executing GetReviews method with unified response.");

        var reviews = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();

        return Ok(ApiResponse<List<ReviewResponse>>.Success(_mapper.Map<List<ReviewResponse>>(reviews)));
    }

    [HttpGet("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewByIdUnified(int id)
    {
        _logger.Information("Executing GetReviewById method with unified response for ID {ReviewId}.", id);

        var review = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found.", id);
            return NotFound();
        }

        return Ok(ApiResponse<ReviewResponse>.Success(_mapper.Map<ReviewResponse>(review)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReviewUnified(ApiRequest<ReviewRequest> request)
    {
        _logger.Information("Executing CreateReview method with unified request for movie ID {MovieId} and user ID {UserId}.", request.Data.MovieId, request.Data.UserId);

        var review = _mapper.Map<Review>(request.Data);
        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<ReviewResponse>.Success(_mapper.Map<ReviewResponse>(review)));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReviewUnified(int id, ApiRequest<ReviewRequest> request)
    {
        _logger.Information("Executing UpdateReview method with unified request for review ID {ReviewId}.", id);

        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found for update.", id);
            return NotFound();
        }

        _mapper.Map(request.Data, review);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<ReviewResponse>.Success(_mapper.Map<ReviewResponse>(review)));
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteReviewUnified(int id)
    {
        _logger.Information("Executing DeleteReview method with unified response for review ID {ReviewId}.", id);

        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse<string>.Success("Review successfully deleted."));
    }
}
