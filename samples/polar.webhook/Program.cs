using Polar.Services;
using PolarNet.Extensions;
using PolarNet.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Use appsettings.Development.json in DEBUG mode

var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#else
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#endif
        .Build();

builder.Configuration.AddConfiguration(config);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTP client for Polar API
builder.Services.AddHttpClient<IPolarService, PolarService>();

// Register Polar service
builder.Services.AddScoped<IPolarService, PolarService>();

// Register Polar webhook services + handler via DI
builder.Services.AddPolarWebhooks<SampleWebhookHandler>(builder.Configuration);

// Register in-memory webhook store for E2E test verification
builder.Services.AddSingleton<IWebhookEventStore, InMemoryWebhookStore>();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Set base URL for success redirects
if (string.IsNullOrEmpty(builder.Configuration["BaseUrl"]))
{
    builder.Configuration["BaseUrl"] = "https://localhost:7123";
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Polar Payment API v1");
    });
}

// Enable raw body buffering for signature verification
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=Index}/{id?}");

// Display startup information to mirror the debugging doc
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var config = app.Services.GetRequiredService<IConfiguration>();
    var urls = config["ASPNETCORE_URLS"] ?? "http://localhost:5000";
    logger.LogInformation("🚀 Webhook Server started at: {Urls}", urls);
    logger.LogInformation("📌 Webhook endpoint: {Endpoint}", $"{urls}/api/webhook/polar");
    logger.LogInformation("💡 ngrok run: ngrok http 5000");
});

app.Run();