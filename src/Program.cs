using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using SecureCodingDemo.Infrastructure;
using SecureCodingDemo.Modules;
using SecureCodingDemo.Services;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "logs"));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(builder.Environment.ContentRootPath, "logs", "secure-coding-demo-.log"),
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddHttpClient("ssrf-demo", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SecureCodingDemo/1.0");
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("login", policy =>
    {
        policy.PermitLimit = 5;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueLimit = 0;
    });
    options.AddConcurrencyLimiter("expensive", policy =>
    {
        policy.PermitLimit = 4;
        policy.QueueLimit = 2;
        policy.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});

var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? "dev-only-demo-signing-key-change-me";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<SafeUrlInspector>();
builder.Services.AddSingleton<ReflectionDemoService>();
builder.Services.AddSingleton<CacheStampedeService>();
builder.Services.AddSingleton<DemoRepository>();
builder.Services.AddSingleton<DocumentationCatalog>();
builder.Services.AddSingleton<DemoScenarioCatalog>();

builder.Services.AddSingleton<RedisConnectionAccessor>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return new RedisConnectionAccessor(null);
    }

    try
    {
        return new RedisConnectionAccessor(ConnectionMultiplexer.Connect(connectionString));
    }
    catch
    {
        return new RedisConnectionAccessor(null);
    }
});

var app = builder.Build();

DemoEnvironmentSetup.EnsureDemoAssets(app.Environment.ContentRootPath);

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<DemoRepository>();
    await repository.InitializeAsync(app.Lifetime.ApplicationStopping);
}

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/demo"))
.ExcludeFromDescription();

app.MapCatalogEndpoints();
app.MapDocumentationEndpoints();
app.MapDemoPlaygroundEndpoints();
app.MapInputSecurityEndpoints();
app.MapDataSecurityEndpoints();
app.MapAvailabilityEndpoints();
app.MapNetworkAndFileEndpoints();
app.MapObservabilityEndpoints();
app.MapConfigurationEndpoints();

app.Run();
