using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;

using StreamAppApi.Contracts.Commands.AuthCommands;
using StreamAppApi.Contracts.Interfaces;

namespace StreamAppApi.App.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] AuthRegisterCommand authRegisterCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        if (!IsValidEmail(authRegisterCommand.email))
        {
            return BadRequest("Invalid email");
        }

        if (authRegisterCommand.password.Length < 6)
        {
            return BadRequest("Invalid password");
        }

        try
        {
            var result = await _authService.RegisterUser(authRegisterCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AuthLoginCommand authLoginCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        if (!IsValidEmail(authLoginCommand.email))
        {
            return BadRequest("Invalid email");
        }

        if (authLoginCommand.password.Length < 6)
        {
            return BadRequest("Invalid password");
        }

        try
        {
            var result = await _authService.LoginUser(authLoginCommand, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login/access-token")]
    public async Task<ActionResult> GetNewTokens([FromBody] AuthGetNewTokensCommand? getNewTokensCommand)
    {
        var cancellationToken = HttpContext?.RequestAborted ?? default;

        try
        {
            //var result = await _authService.GetNewTokens(getNewTokensCommand, cancellationToken);
            var result = await _authService.GetNewTokens(cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // Normalize the domain
            email = Regex.Replace(
                email,
                @"(@)(.+)$",
                DomainMapper,
                RegexOptions.None,
                TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}