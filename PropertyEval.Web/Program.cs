using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Register FastEndpoints and Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();

// Middleware pipeline
app.UseFastEndpoints();
app.UseSwaggerGen();

app.UseHttpsRedirection();

app.Run();