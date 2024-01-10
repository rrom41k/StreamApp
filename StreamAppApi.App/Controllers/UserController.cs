using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StreamAppApi.Contracts.Commands.UserCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    // GET: api/users/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var user = await _userService.GetUserById(userId, cancellationToken);
            
            if (user == null)
                return NotFound();

            return Ok(new { user = user });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    // PUT: api/users/profile
    [HttpPut("profile")]
    public async Task<IActionResult> PutProfile([FromBody] UserUpdateCommand userUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var userId = User.FindFirst("_id")?.Value;
            var updatedUser = await _userService.UpdateUser(userId, userUpdateCommand, cancellationToken);
            return Ok(new { user = updatedUser });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /* Admin Rights */

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserCreateCommand userCreateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var createdUser = await _userService.CreateUser(userCreateCommand, cancellationToken);
            return Ok(new { user = createdUser });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // GET: api/users/
    [HttpGet(), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var users = await _userService.GetAllUsers(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    // GET: api/users/count
    [HttpGet("count"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCount()
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var countUsers = await _userService.GetUsersCount(cancellationToken);
            return Ok(countUsers);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // GET: api/users/{id}
    [HttpGet("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var user = await _userService.GetUserById(id, cancellationToken);
            
            if (user == null)
                return NotFound();
    
            return Ok(new { user = user });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/users/:id
    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutUserById(string id, [FromBody] UserUpdateCommand userUpdateCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var updatedUser = await _userService.UpdateUser(id, userUpdateCommand, cancellationToken);
            return Ok(new { user = updatedUser });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;
        try
        {
            var removedUser = await _userService.DeleteUser(id, cancellationToken);
            return Ok(removedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}