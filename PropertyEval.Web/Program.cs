using FastEndpoints;
using FastEndpoints.Swagger;
using PropertyEval.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

// Register FastEndpoints and Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

var app = builder.Build();

// Middleware pipeline
app.UseFastEndpoints();
app.UseSwaggerGen();

app.UseHttpsRedirection();

app.Run();