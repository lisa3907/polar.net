using Microsoft.AspNetCore.Mvc;
using Polar.Services;
using PolarNet.Models;

namespace Polar.Controllers;

[Route("polar")]
public class PagesController : Controller
{
    private readonly IPolarService _polarService;
    private readonly ILogger<PagesController> _logger;
    private readonly IConfiguration _configuration;

    public PagesController(IPolarService polarService, ILogger<PagesController> logger, IConfiguration configuration)
    {
        _polarService = polarService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await _polarService.GetProductsAsync();
            ViewBag.Products = products.Items;
            ViewBag.BaseUrl = _configuration["BaseUrl"] ?? Request.Scheme + "://" + Request.Host;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading products page");
            ViewBag.Error = "Failed to load products. Please check your Polar configuration.";
            ViewBag.Products = new List<object>();
            return View();
        }
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success([FromQuery] string? checkout_id)
    {
        ViewBag.CheckoutId = checkout_id;

        if (!string.IsNullOrEmpty(checkout_id))
        {
            try
            {
                var checkout = await _polarService.GetCheckoutSessionAsync(checkout_id);
                if (checkout != null)
                {
                    ViewBag.CheckoutStatus = checkout.Status;
                    _logger.LogInformation("Payment success page loaded for checkout {CheckoutId} with status {Status}",
                        checkout_id, checkout.Status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving checkout session {CheckoutId}", checkout_id);
            }
        }

        return View();
    }

    [HttpGet("cancel")]
    public IActionResult Cancel()
    {
        _logger.LogInformation("Payment was canceled by user");
        return View();
    }

    [HttpGet("admin")]
    public async Task<IActionResult> Admin()
    {
        try
        {
            var customers = await _polarService.GetCustomersAsync();
            var subscriptions = await _polarService.GetSubscriptionsAsync();
            
            ViewBag.Customers = customers.Items;
            ViewBag.Subscriptions = subscriptions.Items;
            ViewBag.CustomerCount = customers.Pagination?.TotalCount ?? customers.Items.Count;
            ViewBag.SubscriptionCount = subscriptions.Pagination?.TotalCount ?? subscriptions.Items.Count;
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin page");
            ViewBag.Error = "Failed to load admin data.";
            ViewBag.Customers = new List<object>();
            ViewBag.Subscriptions = new List<object>();
            return View();
        }
    }
}