using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using SophieHR.Api.Data;
using SophieHR.Api.Extensions;
using SophieHR.Api.Models;
using SophieHR.Api.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddJWTTokenServices(builder.Configuration);
builder.Services.TryAddTransient<IEmailSender, EmailService>();
builder.Services.TryAddTransient<ICompanyService, CompanyService>();
builder.Services.TryAddTransient<IDepartmentService, DepartmentService>();
builder.Services.TryAddTransient<IEmployeeService, EmployeeService>();
builder.Services.TryAddScoped<IJobTitleService, JobTitleServiceCache>();

builder.Services.AddMemoryCache();

builder.Services.AddControllers();
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

builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    if (builder.Configuration.GetValue<bool>("ReseedDummyData") && app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            Log.Information("Reseeding the database");
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureDeletedAsync();
            context.Database.Migrate();
            await DataSeeder.Initialize(services);
        }
    }
}

// Register the Swagger generator and the Swagger UI middlewares
app.UseOpenApi();
app.UseSwaggerUi3();

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
        .Enrich.WithMachineName()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
        .Enrich.WithProperty("Environment", environment)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
    var index = new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
    };
    return index;
}