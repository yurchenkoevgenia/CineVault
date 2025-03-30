using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class ReviewsControllerV2 : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger logger;
    private readonly IMapper mapper;

    public ReviewsControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetReviewsUnified(
        ApiRequest request)
    {
        this.logger.Information("Executing GetReviews method with unified response.");

        var reviews = await this.dbContext.Reviews
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

        return this.Ok(ApiResponse.Success(this.mapper.Map<List<ReviewResponse>>(reviews)));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewByIdUnified(
        ApiRequest request, int id)
    {
        this.logger.Information(
            "Executing GetReviewById method with unified response for ID {ReviewId}.", id);

        var review = await this.dbContext.Reviews
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
            this.logger.Warning("Review with ID {ReviewId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(this.mapper.Map<ReviewResponse>(review)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReviewUnified(
        ApiRequest<ReviewRequest> request)
    {
        this.logger.Information(
            "Executing CreateOrUpdateReview method for movie ID {MovieId} and user ID {UserId}.",
            request.Data.MovieId, request.Data.UserId);

        // Перевірка існування користувача та фільму
        bool userExists = await this.dbContext.Users.AnyAsync(u => u.Id == request.Data.UserId);
        bool movieExists = await this.dbContext.Movies.AnyAsync(m => m.Id == request.Data.MovieId);

        if (!userExists || !movieExists)
        {
            return this.NotFound("User or movie not found.");
        }

        if (request.Data.Rating < 1 || request.Data.Rating > 10)
        {
            return this.BadRequest("Rating must be between 1 and 10.");
        }

        var existingReview = await this.dbContext.Reviews
            .FirstOrDefaultAsync(r =>
                r.MovieId == request.Data.MovieId && r.UserId == request.Data.UserId);

        if (existingReview != null)
        {
            this.logger.Information(
                "Updating existing review for movie ID {MovieId} by user ID {UserId}.",
                request.Data.MovieId, request.Data.UserId);

            existingReview.Rating = request.Data.Rating;
            existingReview.Comment = request.Data.Comment;
            await this.dbContext.SaveChangesAsync();

            var updatedReview = await this.dbContext.Reviews
                .Include(r => r.User)
                .Include(r => r.Movie)
                .Include(review => review.Likes)
                .FirstOrDefaultAsync(r => r.Id == existingReview.Id);

            return updatedReview is null
                ? this.NotFound("Updated review not found.")
                : this.Ok(ApiResponse.Success(new ReviewResponse
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

        this.dbContext.Reviews.Add(review);
        await this.dbContext.SaveChangesAsync();

        var createdReview = await this.dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        if (createdReview == null)
        {
            return this.NotFound("Created review not found.");
        }

        return this.Ok(ApiResponse.Success(new { createdReview.Id }));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReviewUnified(int id,
        ApiRequest<ReviewRequest> request)
    {
        this.logger.Information(
            "Executing UpdateReview method with unified request for review ID {ReviewId}.", id);

        var review = await this.dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            this.logger.Warning("Review with ID {ReviewId} not found for update.", id);
            return this.NotFound();
        }

        if (request.Data.Rating < 1 || request.Data.Rating > 10)
        {
            return this.BadRequest("Rating must be between 1 and 10.");
        }

        review.Rating = request.Data.Rating;
        review.Comment = request.Data.Comment;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success(new ReviewResponse
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
    public async Task<ActionResult<ApiResponse<string>>> DeleteReviewUnified(ApiRequest request,
        int id)
    {
        this.logger.Information(
            "Executing DeleteReview method with unified response for review ID {ReviewId}.", id);

        var review = await this.dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this.logger.Warning("Review with ID {ReviewId} not found for deletion.", id);
            return this.NotFound();
        }

        // TODO 10 Реалізувати Soft Delete
        review.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success("Review successfully deleted.", 200));
    }

    [HttpPost("like/{reviewId}/{userId}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> LikeReview(int reviewId, int userId)
    {
        this.logger.Information("User {UserId} is liking review {ReviewId}.", userId, reviewId);

        var review = await this.dbContext.Reviews.Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null)
        {
            this.logger.Warning("Review with ID {ReviewId} not found.", reviewId);
            return this.NotFound("Review not found.");
        }

        bool userExists = await this.dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return this.NotFound("User not found.");
        }

        var existingLike = review.Likes.FirstOrDefault(l => l.UserId == userId);
        if (existingLike != null)
        {
            return this.BadRequest("User has already liked this review.");
        }

        var like = new ReviewLike { ReviewId = reviewId, UserId = userId };
        this.dbContext.ReviewLikes.Add(like);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success("Review liked successfully.", 200));
    }

    [HttpDelete("unlike/{reviewId}/{userId}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> UnlikeReview(int reviewId, int userId)
    {
        this.logger.Information("User {UserId} is unliking review {ReviewId}.", userId, reviewId);

        var like =
            await this.dbContext.ReviewLikes.FirstOrDefaultAsync(l =>
                l.ReviewId == reviewId && l.UserId == userId);
        if (like == null)
        {
            return this.NotFound("Like not found.");
        }

        // TODO 10 Реалізувати Soft Delete
        like.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success("Like removed successfully.", 200));
    }
}