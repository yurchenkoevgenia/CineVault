using Asp.Versioning;
using MapsterMapper;

namespace CineVault.API.Controllers;

// TODO 5 Нова сутність Actor
[ApiVersion(2)]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class ActorsControllerV2 : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger logger;
    private readonly IMapper mapper;

    public ActorsControllerV2(CineVaultDbContext dbContext, ILogger logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpPost("get")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<List<ActorResponse>>>> GetActors(ApiRequest request)
    {
        this.logger.Information("Executing GetActors method with unified response.");
        // TODO 13 Виконати оптимізацію роботи з EFCore
        var actors = await this.dbContext.Actors
            .AsNoTracking()
            .ToListAsync();
        return this.Ok(ApiResponse.Success(this.mapper.Map<List<ActorResponse>>(actors)));
    }

    [HttpPost("get/{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> GetActorById(ApiRequest request,
        int id)
    {
        this.logger.Information(
            "Executing GetActorById method with unified response for actor ID {ActorId}.", id);
        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            this.logger.Warning("Actor with ID {ActorId} not found.", id);
            return this.NotFound();
        }

        return this.Ok(ApiResponse.Success(this.mapper.Map<ActorResponse>(actor)));
    }

    [HttpPost]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> CreateActor(
        ApiRequest<ActorRequest> request)
    {
        this.logger.Information(
            "Executing CreateActor method with unified request");

        var actor = this.mapper.Map<Actor>(request.Data);

        this.dbContext.Actors.Add(actor);
        await this.dbContext.SaveChangesAsync();

        return this.Ok(ApiResponse.Success(new { ActorId = actor.Id }));
    }

    [HttpPut("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> UpdateActor(int id,
        ApiRequest<ActorRequest> request)
    {
        this.logger.Information(
            "Executing UpdateActor method with unified request for actor ID {ActorId}.", id);
        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            this.logger.Warning("Actor with ID {ActorId} not found for update.", id);
            return this.NotFound();
        }

        this.mapper.Map(request.Data, actor);
        await this.dbContext.SaveChangesAsync();
        return this.Ok(ApiResponse.Success(this.mapper.Map<ActorResponse>(actor)));
    }

    [HttpDelete("{id}")]
    [MapToApiVersion(2)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteActor(ApiRequest request, int id)
    {
        this.logger.Information(
            "Executing DeleteActor method with unified response for actor ID {ActorId}.", id);
        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            this.logger.Warning("Actor with ID {ActorId} not found for deletion.", id);
            return this.NotFound();
        }

        // TODO 10 Реалізувати Soft Delete
        actor.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();
        return this.Ok(ApiResponse.Success("Actor deleted successfully", 200));
    }
}