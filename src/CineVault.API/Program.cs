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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RequestTimingMiddleware>();

var app = builder.Build();

app.UseMiddleware<RequestTimingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
