using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Order and Checkout API operations.
    /// </summary>
    public class OrderDemo : DemoBase
    {
        public OrderDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Order & Checkout Demo");

            Console.WriteLine("Select an operation:");
            Console.WriteLine("1. List all orders");
            Console.WriteLine("2. Get order details");
            Console.WriteLine("3. Create checkout session");
            Console.WriteLine("4. Get checkout details");
            Console.Write("\nEnter choice [1-4]: ");
            
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListOrdersAsync();
                        break;
                    case "2":
                        await GetOrderDetailsAsync();
                        break;
                    case "3":
                        await CreateCheckoutAsync();
                        break;
                    case "4":
                        await GetCheckoutDetailsAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Order Demo: {ex.Message}");
            }
        }

        private async Task ListOrdersAsync()
        {
            WriteSubHeader("Listing Orders");
            
            var orders = await Client.ListOrdersAsync(1, 20);
            Console.WriteLine($"Total Orders: {orders.Items.Count}");
            
            if (orders.Items.Count == 0)
            {
                Console.WriteLine("No orders found.");
                return;
            }
            
            foreach (var order in orders.Items.Take(10))
            {
                Console.WriteLine($"\nOrder ID: {order.Id}");
                Console.WriteLine($"  Amount: {order.Amount} {order.Currency}");
                // Note: TaxAmount field is not available in the current model
                Console.WriteLine($"  Created: {order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                
                // Note: UserId field is not available in the current model
                
                if (!string.IsNullOrEmpty(order.ProductId))
                {
                    Console.WriteLine($"  Product ID: {order.ProductId}");
                }
                
                if (!string.IsNullOrEmpty(order.SubscriptionId))
                {
                    Console.WriteLine($"  Subscription ID: {order.SubscriptionId}");
                }
            }
            
            if (orders.Items.Count > 10)
            {
                Console.WriteLine($"\n... and {orders.Items.Count - 10} more orders");
            }
        }

        private async Task GetOrderDetailsAsync()
        {
            WriteSubHeader("Get Order Details");
            
            // List recent orders to help user choose
            var orders = await Client.ListOrdersAsync(1, 5);
            if (orders.Items.Count > 0)
            {
                Console.WriteLine("Recent orders:");
                foreach (var o in orders.Items)
                {
                    Console.WriteLine($"  {o.Id}: {o.Amount} {o.Currency} ({o.CreatedAt:yyyy-MM-dd})");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter order ID: ");
            var orderId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(orderId))
            {
                Console.WriteLine("Order ID is required.");
                return;
            }
            
            var order = await Client.GetOrderAsync(orderId);
            
            Console.WriteLine($"\nOrder Details:");
            Console.WriteLine($"ID: {order.Id}");
            Console.WriteLine($"Amount: {order.Amount} {order.Currency}");
            // Note: TaxAmount field is not available in the current model
            Console.WriteLine($"Created: {order.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Modified: {order.ModifiedAt:yyyy-MM-dd HH:mm:ss}");
            
            // Note: UserId field is not available in the current model
            
            if (!string.IsNullOrEmpty(order.ProductId))
            {
                Console.WriteLine($"Product ID: {order.ProductId}");
            }
            
            if (!string.IsNullOrEmpty(order.PriceId))
            {
                Console.WriteLine($"Price ID: {order.PriceId}");
            }
            
            if (!string.IsNullOrEmpty(order.SubscriptionId))
            {
                Console.WriteLine($"Subscription ID: {order.SubscriptionId}");
            }
            
            if (!string.IsNullOrEmpty(order.CheckoutId))
            {
                Console.WriteLine($"Checkout ID: {order.CheckoutId}");
            }
            
            // Note: BillingAddress field is not available in the current model
        }

        private async Task CreateCheckoutAsync()
        {
            WriteSubHeader("Create Checkout Session");
            
            Console.WriteLine("This will create a checkout session for the default product/price.");
            Console.WriteLine($"Product ID: {ProductId ?? "Not configured"}");
            Console.WriteLine($"Price ID: {PriceId ?? "Not configured"}");
            
            if (string.IsNullOrWhiteSpace(ProductId) || string.IsNullOrWhiteSpace(PriceId))
            {
                Console.WriteLine("\nProduct ID and Price ID must be configured in appsettings.json");
                return;
            }
            
            Console.Write("\nEnter customer email (optional): ");
            var email = Console.ReadLine();
            
            Console.Write("Enter success redirect URL (optional): ");
            var successUrl = Console.ReadLine();
            
            Console.Write("Create checkout session? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Checkout creation cancelled.");
                return;
            }
            
            var checkout = await Client.CreateCheckoutAsync(
                string.IsNullOrWhiteSpace(email) ? null : email,
                string.IsNullOrWhiteSpace(successUrl) ? null : successUrl
            );
            
            Console.WriteLine($"\nCheckout session created!");
            Console.WriteLine($"Checkout ID: {checkout.Id}");
            Console.WriteLine($"Status: {checkout.Status}");
            Console.WriteLine($"Client Secret: {checkout.ClientSecret}");
            Console.WriteLine($"URL: {checkout.Url}");
            Console.WriteLine($"Expires: {checkout.ExpiresAt:yyyy-MM-dd HH:mm:ss}");
            
            Console.WriteLine("\n⚠️ Visit the URL above to complete the checkout process.");
        }

        private async Task GetCheckoutDetailsAsync()
        {
            WriteSubHeader("Get Checkout Details");
            
            Console.Write("Enter checkout ID: ");
            var checkoutId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(checkoutId))
            {
                Console.WriteLine("Checkout ID is required.");
                return;
            }
            
            var checkout = await Client.GetCheckoutAsync(checkoutId);
            
            Console.WriteLine($"\nCheckout Details:");
            Console.WriteLine($"ID: {checkout.Id}");
            Console.WriteLine($"Status: {checkout.Status}");
            Console.WriteLine($"URL: {checkout.Url}");
            Console.WriteLine($"Client Secret: {checkout.ClientSecret}");
            Console.WriteLine($"Created: {checkout.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Expires: {checkout.ExpiresAt:yyyy-MM-dd HH:mm:ss}");
            
            // Note: SuccessUrl, CustomerEmail, PriceId fields are not available in the current model
            
            if (!string.IsNullOrEmpty(checkout.ProductId))
            {
                Console.WriteLine($"Product ID: {checkout.ProductId}");
            }
        }
    }
}