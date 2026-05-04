using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting QuickBite API Gateway...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─── Load Ocelot config file ───────────────────────────────────────────────
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    // ─── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ─── CORS ─────────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:4200"];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("QuickBiteCorsPolicy", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    // ─── Ocelot ───────────────────────────────────────────────────────────────
    builder.Services.AddOcelot();

    // ─── Health Checks ────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "Gateway {RequestMethod} {RequestPath} → {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseCors("QuickBiteCorsPolicy");

    app.MapHealthChecks("/health");

    // ─── Ocelot Middleware ────────────────────────────────────────────────────
    await app.UseOcelot();

    Log.Information("QuickBite API Gateway started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "QuickBite API Gateway failed to start");
}
finally
{
    Log.CloseAndFlush();
}
