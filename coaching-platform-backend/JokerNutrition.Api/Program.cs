using System.Security.Principal;
using System.Text;
using AspNetCoreRateLimit;
using Resend;
using SerilogLog = Serilog.Log;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JokerNutrition.Api.Autofac;
using JokerNutrition.Api.Converters;
using JokerNutrition.Api.Extensions;
using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Hubs;
using JokerNutrition.Business.Autofac;
using JokerNutrition.Business.BackgroundServices;
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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Serilog.Events;

// ─── Bootstrap Serilog ───────────────────────────────────────────────────────
SerilogLog.Logger = new LoggerConfiguration()
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

    // 5. DbContext (PostgreSQL)
    builder.Services.AddDbContext<JokerNutritionContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        )
    ));

    // 6. MemoryCache (needed by rate limiting)
    builder.Services.AddMemoryCache();

    // 7+8. Autofac service provider factory
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
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

    // Register Resend email client (HTTP API — works on Render, unlike SMTP)
    builder.Services.AddResend(o =>
    {
        o.ApiToken = builder.Configuration["SmtpSettings:ResendApiKey"]
                     ?? throw new InvalidOperationException("SmtpSettings:ResendApiKey is not configured.");
    });

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
    builder.Services.AddResponseCaching();

    // 12. Rate Limiting
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // 12b. Hosted Background Services
    builder.Services.AddHostedService<TokenCleanupHostedService>();

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
    else
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                if (exceptionHandlerFeature?.Error != null)
                {
                    Console.WriteLine($"[GLOBAL EXCEPTION] {exceptionHandlerFeature.Error}");
                }

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var responseObj = new
                {
                    statusCode = 500,
                    message = "An unexpected error occurred.",
                    timestamp = DateTime.UtcNow
                };
                var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(responseObj);
                await context.Response.Body.WriteAsync(jsonBytes);
            });
        });
        app.UseHsts();
    }

    // 14. Middleware pipeline
    // Configure forwarded headers to accept X-Forwarded-For & X-Forwarded-Proto from cloud load balancers / ingress proxies (e.g. Cloudflare, Render, Nginx).
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = null // Allow processing through multi-tier cloud ingress proxies
    };
    // Clear default loopback restrictions so cloud proxy headers populate HttpContext.Connection.RemoteIpAddress
    forwardedHeadersOptions.KnownProxies.Clear();
    forwardedHeadersOptions.KnownIPNetworks.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);
    app.UseCors("AllowFrontend");
    app.UseResponseCaching();
    app.UseStaticFiles();
    app.UseHttpsRedirection();
    if (!app.Environment.IsEnvironment("Testing"))
        app.UseIpRateLimiting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    // 15. Enriched health check endpoint — includes DB ping
    app.MapGet("/api/health", async (Microsoft.AspNetCore.Http.HttpContext httpContext, JokerNutrition.Data.Contexts.JokerNutritionContext db) =>
    {
        bool canConnect = false;
        try
        {
            canConnect = await db.Database.CanConnectAsync();
        }
        catch
        {
            // ignore
        }

        var responseObj = new
        {
            status = canConnect ? "healthy" : "degraded",
            database = canConnect ? "connected" : "unreachable",
            timestamp = DateTime.UtcNow
        };

        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "application/json";
        await System.Text.Json.JsonSerializer.SerializeAsync(httpContext.Response.Body, responseObj);
    });

    // 16. Migrate database (always) and seed mock data in dev
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
        if (dbContext.Database.IsRelational())
        {
            int maxRetries = 6;
            int delaySeconds = 10;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    SerilogLog.Information("Applying database migrations (Attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);
                    await dbContext.Database.MigrateAsync();
                    SerilogLog.Information("Database migrations applied successfully.");
                    break;
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        SerilogLog.Error(ex, "Failed to apply database migrations after {MaxRetries} attempts.", maxRetries);
                        throw;
                    }

                    SerilogLog.Warning(ex, "Database migration attempt {Attempt} failed. Retrying in {Delay} seconds...", attempt, delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }

    if (app.Environment.IsDevelopment())
    {
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
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    SerilogLog.Fatal(ex, "Application startup failed.");
}
finally
{
    SerilogLog.CloseAndFlush();
}
