using StreamAppApi.Contracts.Commands.ActorCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IActorService
{
    Task<ActorDto> GetActorBySlug(string slug, CancellationToken cancellationToken);
    Task<List<Dictionary<string, ActorDto>>> GetAllActors(CancellationToken cancellationToken);
    /* Admin Rights */
    Task<ActorDto> CreateActor(ActorCreateCommand actorCreateCommand ,CancellationToken cancellationToken);
    Task<ActorDto> GetActorById(string id, CancellationToken cancellationToken);
    Task<ActorDto> UpdateActor(string id, ActorUpdateCommand actorUpdateCommand, CancellationToken cancellationToken);
    Task<ActorDto> DeleteActor(string id, CancellationToken cancellationToken);
}