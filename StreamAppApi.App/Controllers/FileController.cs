using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/files")]
[ApiController]
[Authorize(Roles = "Admin")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;

    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    // POST: api/file
    [HttpPost]
    public async Task<ActionResult> Post(IFormFileCollection files)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            var result = await _fileService.SaveFiles(
                new(files, HttpContext.Request.Query["folder"]),
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}