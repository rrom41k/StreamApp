using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamAppApi.Contracts.Commands.MovieCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // GET: api/movies/by-slug/:slug
        [HttpGet("by-slug/{slug}")]
        public async Task<IActionResult> GetMovieBySlug(string slug)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var movie = await _movieService.GetMovieBySlug(slug, cancellationToken);

                if (movie == null)
                    return NotFound();

                return Ok(new { movie = movie });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET: api/movies/by-actor/:actorId
        [HttpGet("by-actor/{actorId}")]
        public async Task<IActionResult> GetMovieByActor(string actorId)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var movie = await _movieService.GetMovieByActor(actorId, cancellationToken);

                if (movie == null)
                    return NotFound();

                return Ok(new { movie = movie });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // GET: api/movies/by-genres
        [HttpGet("by-genres")]
        public async Task<IActionResult> GetMovieByActor(List<string> genreIds)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var movies = await _movieService.GetMovieByGenres(genreIds, cancellationToken);

                return Ok(new { movie = movies });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    
        // GET: api/movies/
        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var movies = await _movieService.GetAllMovies(cancellationToken);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // GET: api/movies/most-popular
        [HttpGet("most-popular")]
        public async Task<IActionResult> GetMostPopularMovies()
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var movies = await _movieService.GetMostPopularAsync(cancellationToken);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // GET: api/movies/most-popular
        [HttpPost("update-count-opened")]
        public async Task<IActionResult> UpdateCountOpened([FromBody] string slug)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var movies = await _movieService.UpdateCountOpenedAsync(slug, cancellationToken);
                return Ok(movies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /* Admin Rights */

        // POST: api/movies
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] MovieCreateCommand movieCreateCommand)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var createdMovie = await _movieService.CreateMovie(movieCreateCommand, cancellationToken);
                return Ok(new { movie = createdMovie });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/movies/{id}
        [HttpGet("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMovieById(string id)
        {
            try
            {
                var cancellationToken = HttpContext?.RequestAborted ?? default;
                var movie = await _movieService.GetMovieById(id, cancellationToken);

                if (movie == null)
                    return NotFound();

                return Ok(new { movie = movie });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        // PUT: api/movies/:id
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutMovieById(string id, [FromBody] MovieUpdateCommand movieUpdateCommand)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var updatedMovie = await _movieService.UpdateMovie(id, movieUpdateCommand, cancellationToken);
                return Ok(new { movie = updatedMovie });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/movies/{id} 
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var cancellationToken = HttpContext?.RequestAborted ?? default;
            try
            {
                var removedMovie = await _movieService.DeleteMovie(id, cancellationToken);
                return Ok(removedMovie);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
