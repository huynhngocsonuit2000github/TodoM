using Microsoft.Identity.Client;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/call-todo", async (IConfiguration config, IHttpClientFactory factory) =>
{
    // 🔐 Get token
    var appAuth = ConfidentialClientApplicationBuilder
        .Create(config["AzureAd:ClientId"])
        .WithClientSecret(config["AzureAd:ClientSecret"])
        .WithAuthority($"https://login.microsoftonline.com/{config["AzureAd:TenantId"]}")
        .Build();

    var tokenResult = await appAuth
        .AcquireTokenForClient(new[] { config["AzureAd:Scope"] })
        .ExecuteAsync();

    // 📡 Call Todo API
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);

    var response = await client.GetAsync(
        $"{config["TodoApi:BaseUrl"]}/api/todo"
    );

    return Results.Content(
        await response.Content.ReadAsStringAsync(),
        "application/json"
    );
})
.WithName("CallTodoApi")
.WithOpenApi();

app.Run();
