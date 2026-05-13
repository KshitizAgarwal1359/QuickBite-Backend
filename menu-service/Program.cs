using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuickBite.Menu.Data;
using QuickBite.Menu.Interfaces;
using QuickBite.Menu.Middlewares;
using QuickBite.Menu.Repository;
using QuickBite.Menu.Services;
using Serilog;

// ─── Bootstrap Serilog ───────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting QuickBite Menu Service...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ─── Database (SQL Server locally, PostgreSQL on Render) ─────────────────
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddDbContext<MenuDbContext>(options =>
    {
        if (connectionString.StartsWith("Host=") || connectionString.StartsWith("postgresql://") || connectionString.StartsWith("postgres://"))
            options.UseNpgsql(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Menu"));
        else
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null));
    });

    // ─── JWT Authentication ──────────────────────────────────────────────────
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
    builder.Services.AddScoped<IMenuCategoryRepository, MenuCategoryRepository>();
    builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
    builder.Services.AddScoped<IMenuService, MenuServiceImpl>();

    // ─── Controllers ─────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ─── Swagger / OpenAPI ────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "QuickBite Menu Service API",
            Version     = "v1",
            Description = "Menu management microservice — categories, items, availability toggle, veg/non-veg filter, keyword search.",
            Contact     = new OpenApiContact { Name = "QuickBite Platform" }
        });

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

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    // ─── Health Checks ────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // ─── Auto-migrate in Development ─────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickBite Menu Service v1");
            c.RoutePrefix = "swagger";
            c.DisplayRequestDuration();
        });
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseCors("QuickBiteCorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks("/health");
    app.MapControllers();

    Log.Information("QuickBite Menu Service started successfully on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "QuickBite Menu Service failed to start");
}
finally
{
    Log.CloseAndFlush();
}
