using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.ActorCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class ActorService : IActorService
{
    private readonly string _appSetTok;
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ActorService(
        string appSettingsToken,
        StreamPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        string appSetTok)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _appSetTok = appSetTok;
    }

    public async Task<ActorDto> GetActorBySlug(string slug, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingActor = await _dbContext.Actors.AsNoTracking()
                .FirstOrDefaultAsync(existingActor => existingActor.Slug == slug, cancellationToken)
            ?? throw new ArgumentException("Actor not found.");

        return ActorToDto(existingActor);
    }

    public async Task<List<object>> GetAllActors(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        if (searchTerm == null)
        {
            searchTerm = "";
        }

        var actors = await _dbContext.Actors.AsNoTracking()
            .Where(
                actor =>
                    actor.Name.Contains(searchTerm)
                    || actor.Slug.Contains(searchTerm)
                    || actor.Photo.Contains(searchTerm))
            .ToListAsync(cancellationToken);

        return MapActorsToDto(actors);
    }

    /* Admin Rights */

    public async Task<ActorDto> CreateActor(ActorCreateCommand actorCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var findActor = await _dbContext.Actors.FirstOrDefaultAsync(
            actor =>
                actor.Slug == actorCreateCommand.slug.ToLower());

        if (findActor != null)
        {
            throw new("Actor with this slug contains in DB");
        }

        Actor newActor = new(actorCreateCommand.name, actorCreateCommand.slug, actorCreateCommand.photo);

        _dbContext.Actors.Add(newActor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ActorToDto(newActor);
    }

    public async Task<ActorDto> GetActorById(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingActor = await _dbContext.Actors.AsNoTracking()
                .FirstOrDefaultAsync(existingActor => existingActor.ActorId == id, cancellationToken)
            ?? throw new ArgumentException("Actor not found.");

        return ActorToDto(existingActor);
    }

    public async Task<ActorDto> UpdateActor(
        string id,
        ActorUpdateCommand actorUpdateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var actorToUpdate = await _dbContext.Actors.AsNoTracking()
            .FirstOrDefaultAsync(actorToUpdate => actorToUpdate.ActorId == id, cancellationToken);

        if (actorToUpdate == null)
        {
            throw new ArgumentException("Actor not found.");
        }

        UpdateActorHelper(actorToUpdate, actorUpdateCommand);
        await _dbContext.SaveChangesAsync(cancellationToken);


        return ActorToDto(actorToUpdate);
    }

    public async Task<ActorDto> DeleteActor(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingActor = await _dbContext.Actors.AsNoTracking()
            .FirstOrDefaultAsync(existingActor => existingActor.ActorId == id, cancellationToken);

        if (existingActor == null)
        {
            throw new ArgumentException("Actor not fount.");
        }

        _dbContext.Actors.Remove(existingActor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ActorToDto(existingActor);
    }

    private void UpdateActorHelper(Actor actorToUpdate, ActorUpdateCommand actorUpdateCommand)
    {
        actorToUpdate.Name = actorUpdateCommand.name ?? actorToUpdate.Name;
        actorToUpdate.Slug = actorUpdateCommand.slug.ToLower() ?? actorToUpdate.Slug;
        actorToUpdate.Photo = actorUpdateCommand.photo ?? actorToUpdate.Photo;
    }

    public static ActorDto ActorToDto(Actor actor)
    {
        return new(
            actor.ActorId,
            actor.Name,
            actor.Slug,
            actor.Photo);
    }

    private List<object> MapActorsToDto(List<Actor> actors)
    {
        List<object> actorsListDto = new();

        foreach (var actor in actors)
        {
            var newActor =
                new
                {
                    _id = actor.ActorId,
                    name = actor.Name,
                    slug = actor.Slug,
                    photo = actor.Photo,
                    countMovies = _dbContext.ActorMovies.Count(am => am.ActorId == actor.ActorId)
                };
            actorsListDto.Add(newActor);
        }

        return actorsListDto;
    }

    public static List<ActorDto> MapActorsToDto(ICollection<ActorMovie> actors)
    {
        List<ActorDto> actorsListDto = new();

        foreach (var actor in actors)
        {
            var actorDto = ActorToDto(actor.Actor);
            actorsListDto.Add(actorDto);
        }

        return actorsListDto;
    }
}