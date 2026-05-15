using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SecureCodingDemo.Infrastructure;
using SecureCodingDemo.Modules;
using SecureCodingDemo.Services;
using Serilog;

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
builder.Services.AddSingleton<DemoTopicCatalog>();
builder.Services.AddSingleton<DemoPlaygroundCatalog>();
builder.Services.AddSingleton<DemoScenarioCatalog>();

var app = builder.Build();

DemoEnvironmentSetup.EnsureDemoAssets(app.Environment.ContentRootPath);

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(DocsPathResolver.ResolveDocsRoot(app.Environment)),
    RequestPath = "/docs",
    ContentTypeProvider = CreateDocsContentTypeProvider()
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/demo"))
.ExcludeFromDescription();

app.MapCatalogEndpoints();
app.MapDemoPlaygroundEndpoints();
app.MapInputSecurityEndpoints();
app.MapDataSecurityEndpoints();
app.MapAvailabilityEndpoints();
app.MapNetworkAndFileEndpoints();
app.MapObservabilityEndpoints();
app.MapConfigurationEndpoints();

app.Run();

static FileExtensionContentTypeProvider CreateDocsContentTypeProvider()
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".md"] = "text/markdown; charset=utf-8";
    return provider;
}
