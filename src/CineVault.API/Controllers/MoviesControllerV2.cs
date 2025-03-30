using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class MoviesControllerV2 : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger logger;
    private readonly IMapper mapper;

    public MoviesControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpGet("search")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> SearchMoviesUnified(
        [FromQuery] string? genre,
        [FromQuery] string? title,
        [FromQuery] string? director,
        [FromQuery] int? releaseYear,
        [FromQuery] double? minRating,
        [FromQuery] double? maxRating)
    {
        this.logger.Information("Executing SearchMovies with filters.");

        var query = this.dbContext.Movies.Include(m => m.Reviews).AsQueryable();

        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(m => m.Genre.Contains(genre));
        }

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(m => m.Title.Contains(title));
        }

        if (!string.IsNullOrEmpty(director))
        {
            query = query.Where(m => m.Director.Contains(director));
        }

        if (releaseYear.HasValue)
        {
            query = query.Where(m =>
                m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear.Value);
        }

        if (minRating.HasValue)
        {
            query = query.Where(m =>
                m.Reviews.Any() && m.Reviews.Average(r => r.Rating) >= minRating.Value);
        }

        if (maxRating.HasValue)
        {
            query = query.Where(m =>
                m.Reviews.Any() && m.Reviews.Average(r => r.Rating) <= maxRating.Value);
        }

        var movies = await query.Select(m => new MovieResponse
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            ReleaseDate = m.ReleaseDate,
            Genre = m.Genre,
            Director = m.Director,
            AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = m.Reviews.Count,
            Reviews = m.Reviews.Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = m.Title,
                UserId = r.UserId,
                Username = r.User != null ? r.User.Username : "Unknown",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                LikeCount = r.Likes.Count
            }).ToList()
        }).ToListAsync();

        return this.Ok(ApiResponse.Success(movies,
            $"Found {movies.Count} movies matching criteria."));
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetMoviesUnified(
        ApiRequest request)
    {
        this.logger.Information("Executing GetMovies method with unified response.");

        var movies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Select(m => new MovieResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = m.Reviews.Count,
                Reviews = m.Reviews.Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    MovieId = r.MovieId,
                    MovieTitle = m.Title,
                    UserId = r.UserId,
                    Username = r.User != null ? r.User.Username : "Unknown",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    LikeCount = r.Likes.Count
                }).ToList()
            })
            .ToListAsync();

        return this.Ok(ApiResponse.Success(movies));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieByIdUnified(
        ApiRequest request, int id)
    {
        this.logger.Information(
            "Executing GetMovieById method with unified response for ID {MovieId}.",
            id);

        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Select(m => new MovieResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = m.Reviews.Count,
                Reviews = m.Reviews.Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    MovieId = r.MovieId,
                    MovieTitle = m.Title,
                    UserId = r.UserId,
                    Username = r.User != null ? r.User.Username : "Unknown",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    LikeCount = r.Likes.Count
                }).ToList()
            })
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            this.logger.Warning("Movie with ID {MovieId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(movie));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> CreateMovieUnified(
        ApiRequest<MovieRequest> request)
    {
        this.logger.Information(
            "Executing CreateMovie method with unified request for movie {Title}.",
            request.Data.Title);

        var movie = new Movie
        {
            Title = request.Data.Title,
            Description = request.Data.Description,
            ReleaseDate = request.Data.ReleaseDate,
            Genre = request.Data.Genre,
            Director = request.Data.Director
        };

        await this.dbContext.Movies.AddAsync(movie);
        await this.dbContext.SaveChangesAsync();

        int movieId = movie.Id;

        var response = this.mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse.Success(new { movieId });
        return this.Ok(apiResponse);
    }

    [HttpPost("bulk-create")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> BulkCreateMoviesUnified(
        ApiRequest<BulkMoviesRequest> request)
    {
        this.logger.Information("Executing BulkCreateMovies method with {MovieCount} movies.",
            request.Data.Movies.Count);

        var movies = this.mapper.Map<List<Movie>>(request.Data.Movies);

        await this.dbContext.Movies.AddRangeAsync(movies);
        await this.dbContext.SaveChangesAsync();

        var movieIds = movies.Select(m => m.Id).ToList();

        var response = this.mapper.Map<List<MovieResponse>>(movies);
        var apiResponse = ApiResponse.Success(new { movieIds });
        return this.Ok(apiResponse);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovieUnified(int id,
        ApiRequest<MovieRequest> request)
    {
        this.logger.Information(
            "Executing UpdateMovie method with unified request for movie ID {MovieId}.", id);

        var movie = await this.dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this.logger.Warning("Movie with ID {MovieId} not found for update.", id);
            return this.NotFound();
        }

        movie.Title = request.Data.Title;
        movie.Description = request.Data.Description;
        movie.ReleaseDate = request.Data.ReleaseDate;
        movie.Genre = request.Data.Genre;
        movie.Director = request.Data.Director;

        await this.dbContext.SaveChangesAsync();

        var response = this.mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse.Success(response);
        return this.Ok(apiResponse);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovieUnified(ApiRequest request,
        int id)
    {
        this.logger.Information(
            "Executing DeleteMovie method with unified response for movie ID {MovieId}.", id);

        var movie = await this.dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this.logger.Warning("Movie with ID {MovieId} not found for deletion.", id);
            return this.NotFound();
        }

        // TODO 10 Реалізувати Soft Delete
        movie.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        var apiResponse = ApiResponse.Success("Movie deleted successfully.", 200);
        return this.Ok(apiResponse);
    }

    // TODO 13 Виконати оптимізацію роботи з EFCore
    [HttpPost("bulk-delete")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> BulkDeleteMoviesUnified(
        ApiRequest<List<int>> request)
    {
        this.logger.Information("Executing BulkDeleteMovies method for {MovieCount} movies.",
            request.Data.Count);

        var movies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Where(m => request.Data.Contains(m.Id))
            .ToListAsync();

        var moviesWithReviews = movies
            .Where(m => m.Reviews.Any())
            .Select(m => m.Id)
            .ToList();

        var moviesToDelete = movies
            .Where(m => !m.Reviews.Any())
            .ToList();

        if (moviesToDelete.Count > 0)
        {
            foreach (var movie in moviesToDelete)
            {
                movie.IsDeleted = true;
            }

            await this.dbContext.SaveChangesAsync();
        }

        var response = new
        {
            SuccessfullyDeleted = moviesToDelete.Count,
            DeletedMovieIds = moviesToDelete.Select(m => m.Id).ToList(),
            MoviesWithReviewsIds = moviesWithReviews,
            Message = moviesWithReviews.Count > 0
                ? $"Could not delete {moviesWithReviews.Count} movies with existing reviews"
                : "All selected movies were successfully deleted"
        };

        return this.Ok(ApiResponse.Success(response));
    }

    // TODO 9 Додати такі нові методи в API
    [HttpPost("get/details/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieDetailsResponse>>> GetMovieDetailsUnified(
        ApiRequest request, int id)
    {
        this.logger.Information("Executing GetMovieDetails method for movie ID {MovieId}.", id);

        // TODO 13 Виконати оптимізацію роботи з EFCore
        var movie = await this.dbContext.Movies
            .Select(m => new MovieDetailsResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = m.Reviews.Count,
                LatestReviews = m.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new ReviewResponse
                    {
                        Id = r.Id,
                        MovieId = r.MovieId,
                        MovieTitle = m.Title,
                        UserId = r.UserId,
                        Username = r.User.Username,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        LikeCount = r.Likes.Count
                    }).ToList()
            })
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            this.logger.Warning("Movie with ID {MovieId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(movie));
    }

    // TODO 9 Додати такі нові методи в API
    [HttpPost("search/advanced")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> SearchMoviesUnified(
        ApiRequest<MovieSearchRequest> request)
    {
        this.logger.Information("Executing complex movie search with filters.");

        var query = this.dbContext.Movies
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Data.SearchText))
        {
            string searchText = request.Data.SearchText.ToLower();
            query = query.Where(m =>
                m.Title.ToLower().Contains(searchText) ||
                m.Description.ToLower().Contains(searchText) ||
                m.Director.ToLower().Contains(searchText));
        }

        if (!string.IsNullOrEmpty(request.Data.Genre))
        {
            query = query.Where(m => m.Genre.Contains(request.Data.Genre));
        }

        if (request.Data.MinRating.HasValue)
        {
            query = query.Where(m =>
                m.Reviews.Any() &&
                m.Reviews.Average(r => r.Rating) >= request.Data.MinRating.Value);
        }

        if (request.Data.ReleaseDateFrom.HasValue)
        {
            query = query.Where(m => m.ReleaseDate >= request.Data.ReleaseDateFrom);
        }

        if (request.Data.ReleaseDateTo.HasValue)
        {
            query = query.Where(m => m.ReleaseDate <= request.Data.ReleaseDateTo);
        }

        var movies = await query.Select(m => new MovieResponse
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            ReleaseDate = m.ReleaseDate,
            Genre = m.Genre,
            Director = m.Director,
            AverageRating = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = m.Reviews.Count,
            Reviews = m.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new ReviewResponse
                    {
                        Id = r.Id,
                        MovieId = r.MovieId,
                        MovieTitle = m.Title,
                        UserId = r.UserId,
                        Username = r.User != null ? r.User.Username : "Unknown",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        LikeCount = r.Likes.Count
                    }).ToList()
        })
            .ToListAsync();

        return this.Ok(ApiResponse.Success(movies,
            $"Found {movies.Count} movies matching criteria."));
    }
}