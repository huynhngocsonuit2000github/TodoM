using Microsoft.Identity.Client;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/call-todo", async (
    IConfiguration config,
    IHttpClientFactory factory) =>
{
    var appAuth = ConfidentialClientApplicationBuilder
        .Create(config["AzureAd:ClientId"])
        .WithClientSecret(config["AzureAd:ClientSecret"])
        .WithAuthority($"https://login.microsoftonline.com/{config["AzureAd:TenantId"]}")
        .Build();

    var token = await appAuth
        .AcquireTokenForClient(new[] { config["AzureAd:Scope"] })
        .ExecuteAsync();

    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token.AccessToken);

    var res = await client.GetAsync(
        $"{config["TodoApi:BaseUrl"]}/api/todos/get-all-external"
    );


    return await res.Content.ReadAsStringAsync();
});

app.Run();