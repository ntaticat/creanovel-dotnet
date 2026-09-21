using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Domain.Models;
using FluentValidation;
using WebAPI.Infrastructure.Auth;
using WebAPI.Infrastructure.Extensions;
using WebAPI.Middlewares;
using WebAPI.Services;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// No MVC controllers remain (every area migrated to Minimal API Features), so Swashbuckle
// needs this explicit ApiExplorer registration to discover Minimal API endpoints at all.
services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options =>
{
    var groupName = "v1";
    options.SwaggerDoc(groupName, new OpenApiInfo
    {
        Title = $"Creanovel Web API {groupName}",
        Version = groupName,
        Description = "RESTful API for create, manage and read visual novels",
        Contact = new OpenApiContact
        {
            Name = "Rafael Estrada",
            Email = "ntaticat@gmail.com",
            Url = new Uri("https://github.com/ntaticat")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTKey"])),
        ClockSkew = TimeSpan.Zero
    });

services.AddAuthorization();

var identityCoreBuilder = services.AddIdentityCore<Usuario>();
var identityBuilder = new IdentityBuilder(identityCoreBuilder.UserType, identityCoreBuilder.Services);
identityBuilder.AddEntityFrameworkStores<CreanovelDbContext>();
identityBuilder.AddSignInManager<SignInManager<Usuario>>();

// Orígenes extra (p. ej. el frontend de docker compose en otro puerto): Cors:AllowedOrigins:0, :1... o Cors__AllowedOrigins__0 en el entorno.
var extraCorsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

services.AddCors(options => options.AddDefaultPolicy(
    corsPolicyBuilder =>
    {
        corsPolicyBuilder.WithOrigins(new[] { "http://localhost:4200", "https://creanovel.netlify.app", "https://creanovel.ntaticat.lat" }.Concat(extraCorsOrigins).ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location");
    }
));

services.AddDbContext<CreanovelDbContext>(
    opt => opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
);

services.AddValidatorsFromAssemblyContaining<Program>();

services.AddScoped<JwtTokenService>();

services.AddScoped<FileStorageService>();

// Vertical Slice Architecture scaffolding: any class named exactly "Handler" gets
// registered as Scoped, and any static class named exactly "Endpoint" with a
// public static Map(IEndpointRouteBuilder) method gets invoked to map its routes.
// Coexists with the MediatR/Controllers setup above while each area migrates to
// Features/ one at a time.
services.AddFeatureHandlers(typeof(Program).Assembly);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CreanovelDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1");
    });
}

app.UseCors();

app.UseStaticFiles();

app.UseAuthorization();

app.MapFeatureEndpoints(typeof(Program).Assembly);

app.Run();
