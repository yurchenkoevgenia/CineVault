using Asp.Versioning;
using CineVault.API.Entities;
using MapsterMapper;

namespace CineVault.API.Controllers;

// TODO 4 Реалізувати CRUD для коментарів до відгуків
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class ReviewsControllerV2 : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public ReviewsControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._mapper = mapper;
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetReviewsUnified(ApiRequest request)
    {
        _logger.Information("Executing GetReviews method with unified response.");

        var reviews = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie != null ? r.Movie.Title : "Unknown",
                UserId = r.UserId,
                Username = r.User != null ? r.User.Username : "Unknown",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                LikeCount = r.Likes.Count
            })
            .ToListAsync();

        return Ok(ApiResponse.Success(_mapper.Map<List<ReviewResponse>>(reviews)));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewByIdUnified(ApiRequest request, int id)
    {
        _logger.Information(
            "Executing GetReviewById method with unified response for ID {ReviewId}.", id);

        var review = await _dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Where(r => r.Id == id)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie != null ? r.Movie.Title : "Unknown",
                UserId = r.UserId,
                Username = r.User != null ? r.User.Username : "Unknown",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                LikeCount = r.Likes.Count
            })
            .FirstOrDefaultAsync();

        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found.", id);
            return NotFound();
        }

        return Ok(ApiResponse.Success(_mapper.Map<ReviewResponse>(review)));
    }

    // TODO 4 Додати можливість ставити рейтинг (1-10), коментар необов'язковий
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReviewUnified(ApiRequest<ReviewRequest> request)
    {
        _logger.Information("Executing CreateOrUpdateReview method for movie ID {MovieId} and user ID {UserId}.",
            request.Data.MovieId, request.Data.UserId);

        // Перевірка існування користувача та фільму
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.Data.UserId);
        var movieExists = await _dbContext.Movies.AnyAsync(m => m.Id == request.Data.MovieId);

        if (!userExists || !movieExists)
        {
            return NotFound("User or movie not found.");
        }

        if (request.Data.Rating < 1 || request.Data.Rating > 10)
        {
            return BadRequest("Rating must be between 1 and 10.");
        }

        // TODO 6 Якщо у користувача вже є відгук для цього фільму, оновлюємо його
        var existingReview = await _dbContext.Reviews
            .FirstOrDefaultAsync(r => r.MovieId == request.Data.MovieId && r.UserId == request.Data.UserId);

        if (existingReview != null)
        {
            _logger.Information("Updating existing review for movie ID {MovieId} by user ID {UserId}.",
                request.Data.MovieId, request.Data.UserId);

            existingReview.Rating = request.Data.Rating;
            existingReview.Comment = request.Data.Comment;
            await _dbContext.SaveChangesAsync();

            var updatedReview = await _dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(r => r.Id == existingReview.Id);

            return updatedReview is null
                ? NotFound("Updated review not found.")
                : Ok(ApiResponse.Success(new ReviewResponse
                {
                    Id = updatedReview.Id,
                    MovieId = updatedReview.MovieId,
                    MovieTitle = updatedReview.Movie.Title,
                    UserId = updatedReview.UserId,
                    Username = updatedReview.User.Username,
                    Rating = updatedReview.Rating,
                    Comment = updatedReview.Comment,
                    CreatedAt = updatedReview.CreatedAt,
                    LikeCount = updatedReview.Likes.Count
                }));
        }

        var review = new Review
        {
            MovieId = request.Data.MovieId,
            UserId = request.Data.UserId,
            Rating = request.Data.Rating,
            Comment = request.Data.Comment
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        var createdReview = await _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        if (createdReview == null)
        {
            return NotFound("Created review not found.");
        }

        // TODO 8 Доробити всі методи по створенню
        return Ok(ApiResponse.Success(new {Id = createdReview.Id}));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReviewUnified(int id, ApiRequest<ReviewRequest> request)
    {
        _logger.Information("Executing UpdateReview method with unified request for review ID {ReviewId}.", id);

        var review = await _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found for update.", id);
            return NotFound();
        }

        if (request.Data.Rating < 1 || request.Data.Rating > 10)
        {
            return BadRequest("Rating must be between 1 and 10.");
        }

        review.Rating = request.Data.Rating;
        review.Comment = request.Data.Comment;
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse.Success(new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie.Title,
            UserId = review.UserId,
            Username = review.User.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            LikeCount = review.Likes.Count
        }));
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteReviewUnified(ApiRequest request, int id)
    {
        _logger.Information(
            "Executing DeleteReview method with unified response for review ID {ReviewId}.", id);

        var review = await _dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.Warning("Review with ID {ReviewId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Reviews.Remove(review);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse.Success("Review successfully deleted.", 200));
    }

    // TODO 5 Підтримка лайків для відгуків-коментарів з оцінкою
    [HttpPost("like/{reviewId}/{userId}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> LikeReview(int reviewId, int userId)
    {
        _logger.Information("User {UserId} is liking review {ReviewId}.", userId, reviewId);

        var review = await _dbContext.Reviews.Include(r => r.Likes).FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null)
        {
            _logger.Warning("Review with ID {ReviewId} not found.", reviewId);
            return NotFound("Review not found.");
        }

        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return NotFound("User not found.");
        }

        var existingLike = review.Likes.FirstOrDefault(l => l.UserId == userId);
        if (existingLike != null)
        {
            return BadRequest("User has already liked this review.");
        }

        var like = new ReviewLike { ReviewId = reviewId, UserId = userId };
        _dbContext.ReviewLikes.Add(like);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse.Success("Review liked successfully.", 200));
    }

    [HttpDelete("unlike/{reviewId}/{userId}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> UnlikeReview(int reviewId, int userId)
    {
        _logger.Information("User {UserId} is unliking review {ReviewId}.", userId, reviewId);

        var like = await _dbContext.ReviewLikes.FirstOrDefaultAsync(l => l.ReviewId == reviewId && l.UserId == userId);
        if (like == null)
        {
            return NotFound("Like not found.");
        }

        _dbContext.ReviewLikes.Remove(like);
        await _dbContext.SaveChangesAsync();

        return Ok(ApiResponse.Success("Like removed successfully.", 200));
    }
}