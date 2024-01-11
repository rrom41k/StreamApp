using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.GenreCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/genres")]
[ApiController]
public class GenreController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    // GET: api/genres/by-slug/:slug
    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetGenreBySlug(string slug)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var genre = await _genreService.GetGenreBySlug(slug, cancellationToken);

            if (genre == null)
            {
                return NotFound();
            }

            return Ok(new { genre });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/genres/collections
    [HttpGet("collections")]
    public async Task<IActionResult> GetCollections()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var collections = await _genreService.GetCollections(cancellationToken);

            return Ok(collections);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/genres/
    [HttpGet]
    public async Task<IActionResult> GetAllGenres()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var genres = await _genreService.GetAllGenres(cancellationToken);

            return Ok(genres);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/genres
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] GenreCreateCommand genreCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var createdGenre = await _genreService.CreateGenre(genreCreateCommand, cancellationToken);

            return Ok(new { genre = createdGenre });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/genres/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetGenreById(string id)
    {
        try
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            var genre = await _genreService.GetGenreById(id, cancellationToken);

            if (genre == null)
            {
                return NotFound();
            }

            return Ok(new { genre });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/genres/:id
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutGenreById(string id, [FromBody] GenreUpdateCommand genreUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var updatedGenre = await _genreService.UpdateGenre(id, genreUpdateCommand, cancellationToken);

            return Ok(new { genre = updatedGenre });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/genres/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var removedGenre = await _genreService.DeleteGenre(id, cancellationToken);

            return Ok(removedGenre);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}