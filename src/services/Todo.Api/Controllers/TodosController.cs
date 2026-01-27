using System.Collections.Concurrent;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, TodoItem> Store = new();
    private static int _nextId = 0;

    private readonly ILogger<TodosController> _logger;

    public TodosController(ILogger<TodosController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        _logger.LogInformation("GET /api/todos count={Count}", Store.Count);
        return Ok(Store.Values.OrderBy(x => x.Id));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Store.TryGetValue(id, out var item) ? Ok(item) : NotFound();
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateTodoRequest req)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new { message = "Title is required" });

        var id = Interlocked.Increment(ref _nextId);

        var item = new TodoItem
        {
            Id = id,
            Title = req.Title.Trim(),
            IsDone = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Store[id] = item;

        _logger.LogInformation("POST /api/todos created id={Id} title={Title}", item.Id, item.Title);

        return Created($"/api/todos/{item.Id}", item);
    }
}
