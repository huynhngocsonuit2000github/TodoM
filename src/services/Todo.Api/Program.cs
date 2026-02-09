using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

var tenantId = "";
var clientId = "";
var clientSecret = "";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })

    .AddCookie(options =>
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.ResponseType = "code";
        options.SaveTokens = true;

        // 🔴 THIS IS THE KEY FIX
        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api") && !ctx.Request.Path.StartsWithSegments("/api/auth/login-az"))

            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.HandleResponse(); // stop Azure redirect
            }
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            // 1. Validate both GUID and URI Audiences
            ValidAudiences = new[]
            {
            clientId,
            $"api://{clientId}"
        },

            // 2. Validate both v1 and v2 Issuer formats
            ValidIssuers = new[]
            {
            $"https://login.microsoftonline.com/{tenantId}/v2.0",
            $"https://sts.windows.net/{tenantId}/"
        }
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log the specific issuer that failed so we can see it
                Console.WriteLine($"JWT Auth Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(
            CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("WorkerPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();

        policy.RequireAssertion(context =>
            context.User.HasClaim(c =>
                (c.Type == "roles" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                && c.Value == "Todo.Read"));
    });
});


builder.Services.Configure<OpenIdConnectOptions>(
    OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
        options.NonceCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    });


builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
