using Polar.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTP client for Polar API
builder.Services.AddHttpClient<IPolarService, PolarService>();

// Register Polar service
builder.Services.AddScoped<IPolarService, PolarService>();

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
    logger.LogInformation("💡 ngrok 실행: ngrok http 5000");
});

app.Run();