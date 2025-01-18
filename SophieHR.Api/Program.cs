using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSwag;
using NSwag.Generation.Processors.Security;
using Prometheus;
using Serilog;
using Serilog.Exceptions;
using SophieHR.Api.Data;
using SophieHR.Api.Extensions;
using SophieHR.Api.Interfaces;
using SophieHR.Api.Models;
using SophieHR.Api.Services;
using StackExchange.Redis;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging();
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

// Register the Swagger services
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "SophieHR";
    document.Version = "1.0";
    document.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox: Bearer {your JWT token}."
    });

    document.OperationProcessors.Add(
        new AspNetCoreOperationSecurityScopeProcessor("JWT"));
}
);

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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
}).AddRoles<IdentityRole<Guid>>()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddTransient<DataSeeder>();

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

var app = builder.Build();


// Register the Swagger generator and the Swagger UI middlewares
app.UseOpenApi();
app.UseSwaggerUi();

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
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCompression();

app.MapControllers();

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
}

app.Run();

void ConfigureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
            optional: true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .WriteTo.Debug()
        //.WriteTo.Console()
        .WriteTo.Elasticsearch(new[] { new Uri(configuration["ElasticConfiguration:Uri"]) }, opts =>
        {
            opts.DataStream = new DataStreamName($"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}", $"{environment?.ToLower().Replace(".", "-")}");
            opts.BootstrapMethod = BootstrapMethod.Failure;
        }, transport =>
        {
        })
        .Enrich.WithProperty("Environment", environment)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}