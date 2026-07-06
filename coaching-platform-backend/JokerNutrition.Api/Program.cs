using System.Security.Principal;
using System.Text;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JokerNutrition.Api.Autofac;
using JokerNutrition.Api.Converters;
using JokerNutrition.Api.Extensions;
using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Hubs;
using JokerNutrition.Business.Autofac;
using JokerNutrition.Business.Configurations;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Data.Autofac;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities.Identities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Events;

// ─── Bootstrap Serilog ───────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/joker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 1. Swagger + Bearer JWT definition
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Joker Nutrition API",
            Version = "v1",
            Description = "Elite nutrition and coaching platform API"
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Format: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });

        // Wire up XML doc comments for Swagger UI
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);
    });

    // 2. CORS — origins driven by configuration (lock in Production via appsettings.Production.json)
    var corsOrigins = builder.Configuration.GetSection("CorsAllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5173"];
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
    });

    // 2b. SignalR
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

    // 3. Serilog
    builder.Host.UseSerilog();

    // 4. Identity <User, Role>
    builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<JokerNutritionContext>()
    .AddDefaultTokenProviders();

    // 5. DbContext (SQL Server / LocalDB)
    builder.Services.AddDbContext<JokerNutritionContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 6. MemoryCache (needed by rate limiting)
    builder.Services.AddMemoryCache();

    // 7+8. Autofac service provider factory
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        // Register DbContext
        containerBuilder.RegisterType<JokerNutritionContext>().InstancePerLifetimeScope();

        // Register JwtTokenHelper
        containerBuilder.RegisterType<JwtTokenHelper>().As<IJwtTokenHelper>().InstancePerLifetimeScope();

        // Register IPrincipal from HttpContext
        containerBuilder.Register(ctx =>
        {
            var httpContextAccessor = ctx.Resolve<IHttpContextAccessor>();
            return httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
        }).As<IPrincipal>().InstancePerLifetimeScope();

        // Autofac modules
        containerBuilder.RegisterModule<RepositoriesModule>();
        containerBuilder.RegisterModule<ServicesModule>();
        containerBuilder.RegisterModule<ControllersModule>();
    });

    // 9. Bind configuration sections
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.Configure<BlobStorageSettings>(builder.Configuration.GetSection("BlobStorageSettings"));
    builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    // 10. JWT Bearer Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // 11. Controllers (with UTC DateTime converters to ensure Z suffix)
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();

    // 12. Rate Limiting
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // ─── Build ───────────────────────────────────────────────────────────────
    var app = builder.Build();

    // 13. Swagger (dev only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Joker Nutrition API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // 14. Middleware pipeline
    app.UseCors("AllowFrontend");
    app.UseStaticFiles();
    app.UseHttpsRedirection();
    app.UseIpRateLimiting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    // 15. Enriched health check endpoint — includes DB ping
    app.MapGet("/api/health", async (JokerNutrition.Data.Contexts.JokerNutritionContext db) =>
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync();
            return Results.Ok(new
            {
                status = canConnect ? "healthy" : "degraded",
                database = canConnect ? "connected" : "unreachable",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Results.Ok(new
            {
                status = "degraded",
                database = "error",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    });

    // 16. Migrate and seed mock data in dev
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            await dbContext.Database.MigrateAsync();
        }
        await app.SeedMockDataAsync();

        // Ensure static upload folder exists locally
        var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
}
finally
{
    Log.CloseAndFlush();
}
