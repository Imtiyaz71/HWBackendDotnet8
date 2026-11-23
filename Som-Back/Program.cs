using Microsoft.Extensions.FileProviders;
using Som_Back.Extensions;
using Som_Back.Middlewares;
using Som_Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture Layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddControllers();

var app = builder.Build();

// Custom Middlewares
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Uploads Static File Handling
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads"
});

// Swagger & Root Navigation
app.UseSwaggerDocumentation();
app.MapGet("/", () => Results.Redirect("/swagger"));

// CORS & Auth Pipeline
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
