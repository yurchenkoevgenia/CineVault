using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public MoviesController(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<MovieResponse>>> GetMovies()
    {
        _logger.Information("Executing GetMovies method.");

        var movies = await _dbContext.Movies
            .Include(m => m.Reviews)
            .ToListAsync();

        var response = _mapper.Map<List<MovieResponse>>(movies);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        _logger.Information("Executing GetMovieById method for ID {MovieId}.", id);

        var movie = await _dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            _logger.Warning("Movie with ID {MovieId} not found.", id);
            return NotFound();
        }

        var response = _mapper.Map<MovieResponse>(movie);
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateMovie(MovieRequest request)
    {
        _logger.Information("Executing CreateMovie method for movie {Title}.", request.Title);

        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseDate = request.ReleaseDate,
            Genre = request.Genre,
            Director = request.Director
        };

        await _dbContext.Movies.AddAsync(movie);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        _logger.Information("Executing UpdateMovie method for movie ID {MovieId}.", id);

        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            _logger.Warning("Movie with ID {MovieId} not found for update.", id);
            return NotFound();
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;
        movie.Director = request.Director;

        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        _logger.Information("Executing DeleteMovie method for movie ID {MovieId}.", id);

        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            _logger.Warning("Movie with ID {MovieId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetMoviesUnified()
    {
        _logger.Information("Executing GetMovies method with unified response.");

        var movies = await _dbContext.Movies
            .Include(m => m.Reviews)
            .ToListAsync();

        var response = _mapper.Map<List<MovieResponse>>(movies);
        var apiResponse = ApiResponse<List<MovieResponse>>.Success(response);
        return Ok(apiResponse);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieByIdUnified(int id)
    {
        _logger.Information("Executing GetMovieById method with unified response for ID {MovieId}.", id);

        var movie = await _dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            _logger.Warning("Movie with ID {MovieId} not found.", id);
            return NotFound();
        }

        var response = _mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse<MovieResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> CreateMovieUnified(ApiRequest<MovieRequest> request)
    {
        _logger.Information("Executing CreateMovie method with unified request for movie {Title}.", request.Data.Title);

        var movie = new Movie
        {
            Title = request.Data.Title,
            Description = request.Data.Description,
            ReleaseDate = request.Data.ReleaseDate,
            Genre = request.Data.Genre,
            Director = request.Data.Director
        };

        await _dbContext.Movies.AddAsync(movie);
        await _dbContext.SaveChangesAsync();

        var response = _mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse<MovieResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovieUnified(int id, ApiRequest<MovieRequest> request)
    {
        _logger.Information("Executing UpdateMovie method with unified request for movie ID {MovieId}.", id);

        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            _logger.Warning("Movie with ID {MovieId} not found for update.", id);
            return NotFound();
        }

        movie.Title = request.Data.Title;
        movie.Description = request.Data.Description;
        movie.ReleaseDate = request.Data.ReleaseDate;
        movie.Genre = request.Data.Genre;
        movie.Director = request.Data.Director;

        await _dbContext.SaveChangesAsync();

        var response = _mapper.Map<MovieResponse>(movie);
        var apiResponse = ApiResponse<MovieResponse>.Success(response);
        return Ok(apiResponse);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovieUnified(int id)
    {
        _logger.Information("Executing DeleteMovie method with unified response for movie ID {MovieId}.", id);

        var movie = await _dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            _logger.Warning("Movie with ID {MovieId} not found for deletion.", id);
            return NotFound();
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();

        var apiResponse = ApiResponse<string>.Success("Movie deleted successfully.");
        return Ok(apiResponse);
    }
}
