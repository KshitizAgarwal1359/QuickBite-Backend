using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuickBite.Restaurant.Data;
using QuickBite.Restaurant.Interfaces;
using QuickBite.Restaurant.Middlewares;
using QuickBite.Restaurant.Repository;
using QuickBite.Restaurant.Services;
using Serilog;

// ─── Bootstrap Serilog early so startup errors are captured ──────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting QuickBite Restaurant Service...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ─── Database — EF Core + SQL Server ─────────────────────────────────────
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null)));

    // ─── JWT Authentication (validates tokens issued by auth-service) ─────────
    var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey is missing from configuration");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero,
            RoleClaimType            = System.Security.Claims.ClaimTypes.Role
        };
    });

    builder.Services.AddAuthorization();

    // ─── CORS ─────────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000"];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("QuickBiteCorsPolicy", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // ─── Dependency Injection ─────────────────────────────────────────────────
    builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
    builder.Services.AddScoped<IRestaurantService, RestaurantServiceImpl>();

    // ─── Controllers ─────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ─── Swagger / OpenAPI ────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "QuickBite Restaurant Service API",
            Version     = "v1",
            Description = "Restaurant management microservice for the QuickBite platform. Handles restaurant profiles, geo-proximity search, admin approval, and open/closed toggle.",
            Contact     = new OpenApiContact
            {
                Name = "QuickBite Platform"
            }
        });

        // Add JWT Bearer auth to Swagger UI
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Description  = "Enter your JWT Bearer token: **Bearer {token}**",
            In           = ParameterLocation.Header,
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            Reference    = new OpenApiReference
            {
                Id   = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtSecurityScheme, Array.Empty<string>() }
        });

        // Include XML comments for endpoint documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    // ─── Health Checks ────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────────────────────────

    // ─── Apply EF Core Migrations on startup (Development only) ──────────────
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        db.Database.Migrate();
        Log.Information("Database migration applied successfully");
    }

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickBite Restaurant Service v1");
            c.RoutePrefix = "swagger";
            c.DisplayRequestDuration();
        });
    }

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseCors("QuickBiteCorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("QuickBite Restaurant Service started successfully on {Environment}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "QuickBite Restaurant Service failed to start");
}
finally
{
    Log.CloseAndFlush();
}
