using Polar.Services;

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=Index}/{id?}");

// Display startup information
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var config = app.Services.GetRequiredService<IConfiguration>();
    var useSandbox = config.GetValue<bool>("PolarSettings:UseSandbox", true);
    
    logger.LogInformation("🎯 Polar Payment Test Application Started");
    logger.LogInformation("Environment: {Environment}", useSandbox ? "Sandbox" : "Production");
    logger.LogInformation("Navigate to: https://localhost:7123/polar");
    logger.LogInformation("API Documentation: https://localhost:7123/swagger");
    logger.LogInformation("Admin Dashboard: https://localhost:7123/polar/admin");
    logger.LogInformation("Webhook Endpoint: https://localhost:7123/api/webhook/polar");
    
    if (config["PolarSettings:AccessToken"] == "polar_oat_your_sandbox_token_here")
    {
        logger.LogWarning("⚠️  Please update your Polar Access Token in appsettings.json");
        logger.LogWarning("   Get your sandbox token from: https://sandbox.polar.sh/settings");
    }
});

app.Run();