using System;
using System.Text;
using Application.Commands.Novela;
using Application.Queries.Novela;
using Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Domain.Models;
using MediatR;
using FluentValidation.AspNetCore;
using WebAPI.Middlewares;

namespace WebAPI
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      AddSwagger(services);

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWTKey"])),
            ClockSkew = TimeSpan.Zero

          });

      var builder = services.AddIdentityCore<Usuario>();

      var identityBuilder = new IdentityBuilder(builder.UserType, builder.Services);

      identityBuilder.AddEntityFrameworkStores<CreanovelDbContext>();
      identityBuilder.AddSignInManager<SignInManager<Usuario>>();

      services.AddCors(options => options.AddDefaultPolicy(
          corsPolicyBuilder =>
          {
            corsPolicyBuilder.WithOrigins("http://localhost:4200").AllowAnyHeader();
          }
      ));

      services.AddDbContext<CreanovelDbContext>(
          opt => opt.UseSqlServer(
              Configuration.GetConnectionString("DefaultConnection")
          )
      );
      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

      services.AddControllers()
        .AddFluentValidation( cfg => cfg.RegisterValidatorsFromAssemblyContaining<CreateNovelaCommand>() )
        .AddNewtonsoftJson(
          options => options.SerializerSettings.ReferenceLoopHandling =
              Newtonsoft.Json.ReferenceLoopHandling.Ignore
      );

      services.AddMediatR(typeof(GetNovelasQuery.Handler).Assembly);
    }

    private void AddSwagger(IServiceCollection services)
    {
      services.AddSwaggerGen(options =>
        {
          var groupName = "v1";
          options.SwaggerDoc(groupName, new OpenApiInfo
          {
            Title = $"CreaNovel Web API {groupName}",
            Version = groupName,
            Description = "RESTful API for create, manage and read visual novels",
            Contact = new OpenApiContact
            {
              Name = "Rafael Estrada",
              Email = "ntaticat@gmail.com",
              Url = new Uri("http://github.com/ntaticat")
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
          
          options.AddSecurityRequirement(new OpenApiSecurityRequirement
          {
            {
              new OpenApiSecurityScheme
              {
                Reference = new OpenApiReference
                {
                  Type = ReferenceType.SecurityScheme,
                  Id = "Bearer"
                }
              },
              new string[] {}
            }
          });
        }
      );
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ExceptionHandlerMiddleware>();

      if (env.IsDevelopment())
      {
        // app.UseDeveloperExceptionPage();
      }

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1");
      });

      // app.UseHttpsRedirection();
      app.UseCors();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
