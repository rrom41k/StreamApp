using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.ActorCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/actors")]
[ApiController]
public class ActorController : ControllerBase
{
    private readonly IActorService _actorService;

    public ActorController(IActorService actorService)
    {
        _actorService = actorService;
    }

    // GET: api/actors/by-slug/:slug
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetActorBySlug(string slug)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var actor = await _actorService.GetActorBySlug(slug, cancellationToken);

            if (actor == null)
            {
                return NotFound();
            }

            return Ok(actor);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/actors/
    [HttpGet]
    public async Task<IActionResult> GetAllActors()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var actors = await _actorService.GetAllActors(cancellationToken);

            return Ok(actors);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/actors
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] ActorCreateCommand actorCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdActor = await _actorService.CreateActor(actorCreateCommand, cancellationToken);

            return Ok(createdActor);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/actors/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetActorById(string id)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var actor = await _actorService.GetActorById(id, cancellationToken);

            return Ok(actor);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/actors/:id
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutActorById(string id, [FromBody] ActorUpdateCommand actorUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedActor = await _actorService.UpdateActor(id, actorUpdateCommand, cancellationToken);

            return Ok(updatedActor);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/actors/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var removedActor = await _actorService.DeleteActor(id, cancellationToken);

            return Ok(removedActor);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}