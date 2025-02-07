using Asp.Versioning;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

string? logLevelStr = builder.Configuration["Logging:LogLevel:Default"];

if (logLevelStr == null)
{
    throw new InvalidOperationException("Logging level is not configured");
}

bool isLogLevel = Enum.TryParse<LogEventLevel>(logLevelStr, out var logLevel);

if (!isLogLevel)
{
    throw new InvalidOperationException("Logging level is not correct");
}

builder.Services.AddSerilog(config =>
{
    config
        .MinimumLevel.Is(logLevel)
        .WriteTo.Console()
        .WriteTo.File("Logs/LogFile.txt");
});

builder.Services.AddCineVaultDbContext(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CineVault API V1", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "CineVault API V2", Version = "v2" });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<RequestTimingMiddleware>();

var app = builder.Build();

app.UseMiddleware<RequestTimingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CineVault API V1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "CineVault API V2");
    });
}

if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine($"Active Environment: {app.Environment.EnvironmentName}");

app.Run();
