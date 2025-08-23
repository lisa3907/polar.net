using Microsoft.Extensions.Configuration;
using PolarSample.Demos;

// Load configuration
var config = new ConfigurationBuilder()
#if DEBUG
    .AddJsonFile("appsettings.Development.json", optional: true)
#else
    .AddJsonFile("appsettings.json", optional: false)
#endif
    .Build();

// Validate configuration
var polar = config.GetSection("PolarSettings");
var accessToken = polar["AccessToken"] ?? string.Empty;

if (string.IsNullOrWhiteSpace(accessToken))
{
    Console.WriteLine("⚠️ PolarSettings:AccessToken is missing in appsettings.json");
    Console.WriteLine("Please configure your Polar API credentials before running the demos.");
    return;
}

var useSandbox = string.Equals(polar["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
var baseUrl = useSandbox ? polar["SandboxApiUrl"] : polar["ProductionApiUrl"];

Console.WriteLine("========================================");
Console.WriteLine("     Polar API Client Demo Suite");
Console.WriteLine("========================================");
Console.WriteLine($"Environment: {(useSandbox ? "SANDBOX" : "PRODUCTION")}");
Console.WriteLine($"Base URL: {baseUrl}");
Console.WriteLine();

// Main menu loop
while (true)
{
    DisplayMainMenu();
    var choice = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(choice))
        continue;

    try
    {
        DemoBase? demo = choice switch
        {
            "1" => new ProductDemo(config),
            "2" => new CustomerDemo(config),
            "3" => new OrderDemo(config),
            "4" => new SubscriptionDemo(config),
            "5" => new PaymentDemo(config),
            "6" => new WebhookDemo(config),
            "7" => new BenefitDemo(config),
            "8" => new OrganizationDemo(config),
            "0" or "q" or "Q" => null,
            _ => null
        };

        if (choice == "0" || choice?.ToLower() == "q")
        {
            Console.WriteLine("\nThank you for using Polar API Demo Suite!");
            break;
        }

        if (demo != null)
        {
            await demo.RunAsync();
            
            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("Invalid choice. Please try again.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error occurred: {ex.Message}");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    Console.Clear();
}

static void DisplayMainMenu()
{
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine("║           MAIN MENU                    ║");
    Console.WriteLine("╠════════════════════════════════════════╣");
    Console.WriteLine("║  1. Product & Price Management         ║");
    Console.WriteLine("║  2. Customer Management                ║");
    Console.WriteLine("║  3. Order & Checkout                   ║");
    Console.WriteLine("║  4. Subscription Management            ║");
    Console.WriteLine("║  5. Payment & Refund                   ║");
    Console.WriteLine("║  6. Webhook Management                 ║");
    Console.WriteLine("║  7. Benefits                           ║");
    Console.WriteLine("║  8. Organization Info                  ║");
    Console.WriteLine("║                                        ║");
    Console.WriteLine("║  0. Exit (or Q)                        ║");
    Console.WriteLine("╚════════════════════════════════════════╝");
    Console.Write("\nSelect an option: ");
}