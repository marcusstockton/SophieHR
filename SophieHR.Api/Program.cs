using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prometheus;
using Scalar.AspNetCore;
using Serilog;
using SophieHR.Api;
using SophieHR.Api.Data;
using SophieHR.Api.Extensions;
using SophieHR.Api.Interfaces;
using SophieHR.Api.Models;
using SophieHR.Api.Services;
using StackExchange.Redis;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddJWTTokenServices(builder.Configuration);
builder.Services.TryAddTransient<IEmailSender, EmailService>();
builder.Services.TryAddScoped<ICompanyService, CompanyService>();
builder.Services.TryAddScoped<IDepartmentService, DepartmentService>();
builder.Services.TryAddScoped<IEmployeeService, EmployeeService>();
builder.Services.TryAddScoped<IJobTitleService, JobTitleServiceCache>();

builder.Services.AddMemoryCache();

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCustomOpenApi();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
        .WithOrigins("http://localhost:4200")
        //.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CompanyManagement", policy =>
          policy.RequireRole("Admin", "CompanyAdmin", "HRManager", "Manager"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Ensure a connection string is provided; fail-fast if missing so misconfiguration is obvious
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Set the ConnectionStrings__DefaultConnection environment variable or provide it in configuration.");
}

// Diagnostic: parse host(s) from the connection string and verify DNS resolution early with clear logging.
try
{
    var npg = new Npgsql.NpgsqlConnectionStringBuilder(defaultConnection);
    var hosts = (npg.Host ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    foreach (var host in hosts)
    {
        try
        {
            var addrs = System.Net.Dns.GetHostAddresses(host);
            Log.Information("Resolved DB host '{Host}' to {Addresses}", host, string.Join(",", addrs.Select(a => a.ToString())));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to resolve DB host '{Host}' during startup diagnostics", host);
            throw new InvalidOperationException($"Unable to resolve DB host '{host}'. Ensure the service name is correct and both containers share a network.", ex);
        }
    }
}
catch (InvalidOperationException)
{
    // rethrow configuration errors
    throw;
}
catch (Exception ex)
{
    Log.Error(ex, "Unexpected error parsing DB connection string during startup diagnostics");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(defaultConnection);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole<Guid>>()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddTransient<DataSeeder>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var pd = new ValidationProblemDetails(context.ModelState)
        {
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };
        return new JsonResult(pd) { StatusCode = StatusCodes.Status400BadRequest };
    };
});
builder.Services.AddHttpClient("autosuggestHereApiClient", client =>
{
    var url = builder.Configuration.GetSection("ThirdPartyClients:HereApi").GetValue<string>("BaseUrl");
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("imageHereApiClient", client =>
{
    var url = builder.Configuration.GetSection("ThirdPartyClients:HereApiImages").GetValue<string>("BaseUrl");
    client.BaseAddress = new Uri(url);
});

builder.Services.AddHttpClient("postcodesioClient", client =>
{
    var url = builder.Configuration.GetSection("ThirdPartyClients:PostcodesIo").GetValue<string>("BaseUrl");
    client.BaseAddress = new Uri(url);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis_cache:6379";
    options.ConfigurationOptions = new ConfigurationOptions()
    {
        AbortOnConnectFail = true,
        EndPoints = { options.Configuration }
    };
});

builder.Services.AddResponseCaching();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

var app = builder.Build();

app.UseOpenApi();

app.UseMetricServer();

app.Use((context, next) =>
{
    // Http Context
    var counter = Metrics.CreateCounter("PathCounter", "Count request", new CounterConfiguration { LabelNames = new[] { "method", "endpoint" } });
    // method: GET, POST etc.
    // endpoint: Requested path
    counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
    return next();
});

app.UseCors("CorsPolicy");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCompression();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    if (builder.Configuration.GetValue<bool>("ReseedDummyData"))
    {
        using (var scope = app.Services.CreateScope())
        {
            Log.Information("Reseeding the database");
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            await DataSeeder.Initialize(services);
        }
    }
    // Map controllers first so the OpenAPI generator can discover endpoints
    app.MapControllers();

    var openApiEndpoint = app.MapOpenApi(); // maps to /openapi/v1.json
    openApiEndpoint?.AllowAnonymous();

    var scalarendpoint = app.MapScalarApiReference(option =>
    {
        option.Title = "SophieHR API";
        option.AddDocument("v1", "API Version 1.0", "/openapi/v1.json", isDefault: true);
    });
    scalarendpoint?.AllowAnonymous(); // maps to /scalar

}

// Apply any migrations to the docker image
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.Run();
