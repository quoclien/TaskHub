using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskHub.Api.Common.Swagger;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Core.Services;
using TaskHub.Api.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Serilog;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- ConfigureServices ---

#region Logging - Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

#region Database
// In Testing environment, the test host will configure the DbContext (usually SQLite).
// Avoid registering a provider here to prevent multiple provider conflicts.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }), poolSize: 1024);
    builder.Services.AddScoped<DbContext, ApplicationDbContext>();
}
#endregion

#region Repositories and Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IListRepository, ListRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IListService, ListService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

#region Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Name // Ensures User.Identity.Name is set from name claim
    };
});
#endregion

#region AutoMapper
builder.Services.AddAutoMapper(typeof(TaskHub.Api.Infrastructure.Mapping.BoardProfile).Assembly);
#endregion

#region FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
#endregion

#region Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
#endregion

#region API Versioning & Swagger
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwaggerDefaultValues>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Removed manual c.SwaggerDoc("v1", ...) block to avoid duplicate key error

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Simply enter your token below. Example: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // changed from ApiKey to Http
        Scheme = "bearer",              // lowercase 'bearer' per specification
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                Scheme = "bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Enable annotations for better documentation
    c.EnableAnnotations();
});
#endregion

#region Controllers & CORS
builder.Services.AddScoped<DevelopmentOnlyFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<DevelopmentOnlyFilter>();
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000", "http://localhost:3001", "https://localhost:3000", "https://localhost:3001", "http://127.0.0.1:3000", "http://127.0.0.1:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
#endregion

#region HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<ApplicationDbContext>("database", tags: new[] { "ready" });
#endregion

// --- Build App ---
var app = builder.Build();

// --- Configure Pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Global exception middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed data in development
if (app.Environment.IsDevelopment())
{
    await app.SeedAsync();
}

app.Run();

// Make the implicit Program class public so it can be referenced by test projects
public partial class Program { }
