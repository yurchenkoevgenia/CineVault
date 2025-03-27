using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class MoviesControllerV2 : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public MoviesControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._mapper = mapper;
    }

    // TODO 3 Пошук фільмів
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
        _logger.Information("Executing SearchMovies with filters.");

        var query = _dbContext.Movies.Include(m => m.Reviews).AsQueryable();

        // Фільтрація за жанром
        if (!string.IsNullOrEmpty(genre))
            query = query.Where(m => m.Genre.Contains(genre));

        // Фільтрація за назвою
        if (!string.IsNullOrEmpty(title))
            query = query.Where(m => m.Title.Contains(title));

        // Фільтрація за режисером
        if (!string.IsNullOrEmpty(director))
            query = query.Where(m => m.Director.Contains(director));

        // Фільтрація за роком випуску
        if (releaseYear.HasValue)
            query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear.Value);

        // Фільтрація за середнім рейтингом
        if (minRating.HasValue)
            query = query.Where(m => m.Reviews.Any() && m.Reviews.Average(r => r.Rating) >= minRating.Value);

        if (maxRating.HasValue)
            query = query.Where(m => m.Reviews.Any() && m.Reviews.Average(r => r.Rating) <= maxRating.Value);

        // Виконання запиту
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

        return Ok(ApiResponse.Success(movies, $"Found {movies.Count} movies matching criteria."));
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetMoviesUnified(ApiRequest request)
    {
        this._logger.Information("Executing GetMovies method with unified response.");

        var movies = await this._dbContext.Movies
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
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieByIdUnified(ApiRequest request, int id)
    {
        this._logger.Information("Executing GetMovieById method with unified response for ID {MovieId}.",
            id);

        var movie = await this._dbContext.Movies
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
            this._logger.Warning("Movie with ID {MovieId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(movie));
    }

    // TODO 8 Доробити всі методи по створенню
    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> CreateMovieUnified(ApiRequest<MovieRequest> request)
    {
        this._logger.Information("Executing CreateMovie method with unified request for movie {Title}.",
        request.Data.Title);

        var movie = new Movie
        {
            Title = request.Data.Title,
            Description = request.Data.Description,
            ReleaseDate = request.Data.ReleaseDate,
            Genre = request.Data.Genre,
            Director = request.Data.Director
        };

        await this._dbContext.Movies.AddAsync(movie);
        await this._dbContext.SaveChangesAsync();

        var movieId = movie.Id;

        var response = this._mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse.Success(new { movieId });
        return this.Ok(apiResponse);
    }

    // TODO 1 Додати реалізацію масового завантаження фільмів
    [HttpPost("bulk-create")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> BulkCreateMoviesUnified(ApiRequest<BulkMoviesRequest> request)
    {
        this._logger.Information("Executing BulkCreateMovies method with {MovieCount} movies.", request.Data.Movies.Count);

        var movies = this._mapper.Map<List<Movie>>(request.Data.Movies);

        await this._dbContext.Movies.AddRangeAsync(movies);
        await this._dbContext.SaveChangesAsync();

        var movieIds = movies.Select(m => m.Id).ToList();

        var response = this._mapper.Map<List<MovieResponse>>(movies);
        var apiResponse = ApiResponse.Success(new { movieIds });
        return this.Ok(apiResponse);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovieUnified(int id, ApiRequest<MovieRequest> request)
    {
        this._logger.Information(
            "Executing UpdateMovie method with unified request for movie ID {MovieId}.", id);

        var movie = await this._dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this._logger.Warning("Movie with ID {MovieId} not found for update.", id);
            return this.NotFound();
        }

        movie.Title = request.Data.Title;
        movie.Description = request.Data.Description;
        movie.ReleaseDate = request.Data.ReleaseDate;
        movie.Genre = request.Data.Genre;
        movie.Director = request.Data.Director;

        await this._dbContext.SaveChangesAsync();

        var response = this._mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse.Success(response);
        return this.Ok(apiResponse);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovieUnified(ApiRequest request, int id)
    {
        this._logger.Information(
            "Executing DeleteMovie method with unified response for movie ID {MovieId}.", id);

        var movie = await this._dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this._logger.Warning("Movie with ID {MovieId} not found for deletion.", id);
            return this.NotFound();
        }

        this._dbContext.Movies.Remove(movie);
        await this._dbContext.SaveChangesAsync();

        var apiResponse = ApiResponse.Success("Movie deleted successfully.", 200);
        return this.Ok(apiResponse);
    }

    // TODO 7 Масове видалення фільмів
    [HttpPost("bulk-delete")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> BulkDeleteMoviesUnified(ApiRequest<List<int>> request)
    {
        this._logger.Information("Executing BulkDeleteMovies method for {MovieCount} movies.", request.Data.Count);

        var movieIds = request.Data;
        var moviesToDelete = new List<Movie>();
        var moviesWithReviews = new List<int>();

        // TODO 7 Додати перевірку, чи є фільми у відгуках, перед видаленням
        foreach (var movieId in movieIds)
        {
            var movie = await _dbContext.Movies.Include(m => m.Reviews).FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie != null && movie.Reviews.Any())
            {
                // TODO 7 Якщо є, то не видаляти такий, а виводити попередження, а інші фільми з масиву видалити
                moviesWithReviews.Add(movieId);
            }
            else
            {
                moviesToDelete.Add(movie);
            }
        }

        // Видаляємо фільми без відгуків
        if (moviesToDelete.Any())
        {
            _dbContext.Movies.RemoveRange(moviesToDelete);
            await _dbContext.SaveChangesAsync();
        }

        // Формуємо відповідь
        var response = new
        {
            SuccessMessage = $"Successfully deleted {moviesToDelete.Count} movies.",
            DeletedMovies = moviesToDelete.Select(m => m.Id).ToList(),
            MoviesWithReviews = moviesWithReviews
        };

        return Ok(ApiResponse.Success(response));
    }
}