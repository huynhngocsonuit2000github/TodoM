using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult LoginAsync(LoginModel model)
    {
        if (model.Username == "admin" && model.Password == "123")
            return Ok(new { token = "admin" });

        if (model.Username == "member" && model.Password == "123")
            return Ok(new { token = "member" });

        return BadRequest("Invalid credential!");
    }
    [HttpGet("login-az")]
    public IActionResult LoginAzureAsync()
    {
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = "http://localhost:4200/about"
            },
            OpenIdConnectDefaults.AuthenticationScheme
        );
    }

    [Authorize]
    [HttpGet("authenticated-user")]
    public IActionResult GetAuthenticatedUserInfoAsync(string token)
    {
        if (token == "admin")
            return Ok(new AuthUser() { Id = 1, Name = "User admin", Roles = new List<string>() { "admin", "member" } });

        if (token == "member")
            return Ok(new AuthUser() { Id = 2, Name = "User member", Roles = new List<string>() { "member" } });

        return Unauthorized();
    }
}
