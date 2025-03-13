using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public MoviesController(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this._dbContext = dbContext;
        this._logger = logger;
        this._mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion(1)]
    public async Task<ActionResult<List<MovieResponse>>> GetMovies()
    {
        this._logger.Information("Executing GetMovies method.");

        var movies = await this._dbContext.Movies
            .Include(m => m.Reviews)
            .ToListAsync();

        var response = this._mapper.Map<List<MovieResponse>>(movies);
        return this.Ok(response);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        this._logger.Information("Executing GetMovieById method for ID {MovieId}.", id);

        var movie = await this._dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null)
        {
            this._logger.Warning("Movie with ID {MovieId} not found.", id);
            return this.NotFound();
        }

        var response = this._mapper.Map<MovieResponse>(movie);
        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion(1)]
    public async Task<ActionResult> CreateMovie(MovieRequest request)
    {
        this._logger.Information("Executing CreateMovie method for movie {Title}.", request.Title);

        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseDate = request.ReleaseDate,
            Genre = request.Genre,
            Director = request.Director
        };

        await this._dbContext.Movies.AddAsync(movie);
        await this._dbContext.SaveChangesAsync();

        return this.CreatedAtAction(nameof(this.GetMovieById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        this._logger.Information("Executing UpdateMovie method for movie ID {MovieId}.", id);

        var movie = await this._dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this._logger.Warning("Movie with ID {MovieId} not found for update.", id);
            return this.NotFound();
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;
        movie.Director = request.Director;

        await this._dbContext.SaveChangesAsync();

        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(1)]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        this._logger.Information("Executing DeleteMovie method for movie ID {MovieId}.", id);

        var movie = await this._dbContext.Movies.FindAsync(id);

        if (movie is null)
        {
            this._logger.Warning("Movie with ID {MovieId} not found for deletion.", id);
            return this.NotFound();
        }

        this._dbContext.Movies.Remove(movie);
        await this._dbContext.SaveChangesAsync();

        return this.Ok();
    }
}