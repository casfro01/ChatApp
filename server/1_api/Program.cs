using System.Text.Json;
using System.Text.Json.Serialization;
using _1_api;
using _2_service;
using _2_service.Security;
using _3_dataaccess;
using DefaultNamespace;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors.Security;
using StackExchange.Redis;
using StateleSSE.AspNetCore.Extensions;

namespace api;
public class Program
{
    public static void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        var redis = builder.Configuration.GetSection("Redis").Value;
        services.AddSingleton<AppOptions>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var appOptions = new AppOptions();
            configuration.GetSection(nameof(AppOptions)).Bind(appOptions);
            return appOptions;
        });
        services.AddOpenApiDocument(config =>
        {
            config.AddSecurity("Bearer", new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            });
            config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
            config.AddStringConstants<MyConstants>();
        });

        // repos
        
        // services
        services.AddScoped<ITokenService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddDbContext<MyDbContext>((services, options) =>
        {
            options.UseNpgsql(services.GetRequiredService<AppOptions>().DbConnectionString)
                /*.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)*/;
        });
        
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtService.ValidationParameters(
                    builder.Configuration
                );
                // Add this for debugging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    },
                };
            });
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                // Globally require users to be authenticated
                .RequireAuthenticatedUser()
                .Build();
        });

        services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(0));
        
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = ConfigurationOptions.Parse(redis);
            config.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(config);
        });

        services.AddRedisSseBackplane();
        
        services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.WriteIndented = true;
        });
        
        services.AddCors();
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services, builder);
        
        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.Database.EnsureCreated();
        }
        
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        
        //app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x => true));

        // config færdig her
        app.GenerateApiClientsFromOpenApi("/../../client/src/core/ServerAPI.ts").GetAwaiter().GetResult();
        // for development
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            ctx.Database.EnsureCreated();
            //scope.ServiceProvider.GetRequiredService<ISeeder>().Seed().GetAwaiter().GetResult();
        }
        
        app.Run();
        
    }
}