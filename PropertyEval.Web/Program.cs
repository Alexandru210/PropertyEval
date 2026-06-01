using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using PropertyEval.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PropertyEval";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PropertyEval";

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register FastEndpoints and Swagger
builder.Services
    .AddAuthenticationJwtBearer(
        signingOptions => signingOptions.SigningKey = jwtSecret,
        bearerOptions =>
        {
            bearerOptions.TokenValidationParameters.ValidateIssuer = true;
            bearerOptions.TokenValidationParameters.ValidIssuer = jwtIssuer;
            bearerOptions.TokenValidationParameters.ValidateAudience = true;
            bearerOptions.TokenValidationParameters.ValidAudience = jwtAudience;
            bearerOptions.TokenValidationParameters.ValidateLifetime = true;
            bearerOptions.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(1);
            bearerOptions.TokenValidationParameters.RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        })
    .AddAuthorization()
    .AddFastEndpoints();

builder.Services.Configure<JwtCreationOptions>(options =>
{
    options.SigningKey = jwtSecret;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
});

builder.Services.SwaggerDocument();

var app = builder.Build();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();
