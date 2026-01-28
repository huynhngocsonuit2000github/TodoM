using System.Collections.Concurrent;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new
        {
            role = new List<string>()
            {
                // "admin", // can delete
                "member", // read only
            }
        });
    }
}
