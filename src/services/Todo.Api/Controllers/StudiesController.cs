using System.Collections.Concurrent;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/studies")]
public class StudiesController : ControllerBase
{
    // email -> (id -> study)
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Study>> Store = new();
    private static int _nextId = 0;

    private readonly ILogger<StudiesController> _logger;
    private readonly HttpClient _http = new();

    public StudiesController(ILogger<StudiesController> logger)
    {
        _logger = logger;
    }

    public class CreateStudyRequest
    {
        public string PatientName { get; set; }
    }

    // =========================
    // Cookie Helpers
    // =========================

    private string GetUserEmail()
    {
        if (!Request.Cookies.TryGetValue("TODO_custom_cookies", out var raw))
            throw new UnauthorizedAccessException("Auth cookie missing");

        var ctx = JsonSerializer.Deserialize<AppContextCookie>(raw);

        if (string.IsNullOrWhiteSpace(ctx?.Email))
            throw new UnauthorizedAccessException("Email missing in cookie");

        return ctx.Email;
    }

    private ConcurrentDictionary<int, Study> GetUserStore()
    {
        var email = GetUserEmail();
        return Store.GetOrAdd(email, _ => new ConcurrentDictionary<int, Study>());
    }

    // =========================
    // GET ALL
    // =========================

    [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        var userStore = GetUserStore();
        return Ok(userStore.Values.OrderBy(x => x.Id));
    }

    // =========================
    // CREATE
    // =========================

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
    [FromForm] IFormFile file,
    [FromForm] string patientName)
    {
        if (file == null || file.Length == 0)
            return BadRequest("DICOM file is required");

        var userStore = GetUserStore();
        var id = Interlocked.Increment(ref _nextId);

        try
        {
            // 🔐 Basic Auth
            var byteArray = System.Text.Encoding.ASCII
                .GetBytes("orthanc:orthanc");

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(byteArray));

            // Upload DICOM
            using var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/dicom");

            var uploadResponse = await _http.PostAsync(
                "http://localhost:8042/instances",
                streamContent);

            uploadResponse.EnsureSuccessStatusCode();

            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            using var uploadDoc = JsonDocument.Parse(uploadJson);

            var orthancStudyId = uploadDoc.RootElement
                .GetProperty("ParentStudy")
                .GetString();

            // Get Study info
            var studyResponse = await _http.GetAsync(
                $"http://localhost:8042/studies/{orthancStudyId}");

            studyResponse.EnsureSuccessStatusCode();

            var studyJson = await studyResponse.Content.ReadAsStringAsync();
            using var studyDoc = JsonDocument.Parse(studyJson);

            var studyInstanceUID = studyDoc.RootElement
                .GetProperty("MainDicomTags")
                .GetProperty("StudyInstanceUID")
                .GetString();

            var study = new Study
            {
                Id = id,
                PatientName = patientName ?? "Unknown",
                StudyDate = DateTime.UtcNow,
                StudyInstanceUID = studyInstanceUID!,
                OrthancStudyId = orthancStudyId!,
                CreatedAt = DateTimeOffset.UtcNow
            };

            userStore[id] = study;

            return Created($"/api/studies/{study.Id}", study);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // =========================
    // DELETE
    // =========================

    [Authorize(Roles = "AdminAZ")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userStore = GetUserStore();

        if (!userStore.TryRemove(id, out var study))
            return NotFound();

        // Delete from Orthanc
        await _http.DeleteAsync(
            $"http://localhost:8042/studies/{study.OrthancStudyId}");

        return Ok(new { status = "Deleted", value = id });
    }
}