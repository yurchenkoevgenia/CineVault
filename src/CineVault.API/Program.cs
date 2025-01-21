using Asp.Versioning;
using CineVault.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddCineVaultDbContext(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CineVault API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "CineVault API V2");
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
