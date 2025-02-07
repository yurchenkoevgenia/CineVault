using Asp.Versioning;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;

    public ReviewsController(CineVaultDbContext dbContext, ILogger logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        _logger.Information("Executing GetReviews method.");

        var reviews = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie!.Title,
                UserId = r.UserId,
                Username = r.User!.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
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

        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        _logger.Information("Executing CreateReview method for movie ID {MovieId} and user ID {UserId}.", request.MovieId, request.UserId);

        var review = new Review
        {
            MovieId = request.MovieId,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment
        };

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

        review.MovieId = request.MovieId;
        review.UserId = request.UserId;
        review.Rating = request.Rating;
        review.Comment = request.Comment;

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
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie!.Title,
                UserId = r.UserId,
                Username = r.User!.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var response = ApiResponse<List<ReviewResponse>>.Success(reviews);
        return Ok(response);
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

        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        var apiResponse = ApiResponse<ReviewResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReviewUnified(ApiRequest<ReviewRequest> request)
    {
        _logger.Information("Executing CreateReview method with unified request for movie ID {MovieId} and user ID {UserId}.", request.Data.MovieId, request.Data.UserId);

        var review = new Review
        {
            MovieId = request.Data.MovieId,
            UserId = request.Data.UserId,
            Rating = request.Data.Rating,
            Comment = request.Data.Comment
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        var apiResponse = ApiResponse<ReviewResponse>.Success(response);
        return Ok(apiResponse);
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

        review.MovieId = request.Data.MovieId;
        review.UserId = request.Data.UserId;
        review.Rating = request.Data.Rating;
        review.Comment = request.Data.Comment;

        await _dbContext.SaveChangesAsync();

        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };

        var apiResponse = ApiResponse<ReviewResponse>.Success(response);
        return Ok(apiResponse);
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
