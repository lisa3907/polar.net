using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Subscription API operations.
    /// </summary>
    public class SubscriptionDemo : DemoBase
    {
        public SubscriptionDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Subscription Management Demo");

            Console.WriteLine("Select a subscription operation:");
            Console.WriteLine("1. List all subscriptions");
            Console.WriteLine("2. Get subscription details");
            Console.WriteLine("3. Create subscription");
            Console.WriteLine("4. Cancel subscription");
            Console.WriteLine("5. Revoke subscription");
            Console.Write("\nEnter choice [1-5]: ");
            
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListSubscriptionsAsync();
                        break;
                    case "2":
                        await GetSubscriptionDetailsAsync();
                        break;
                    case "3":
                        await CreateSubscriptionAsync();
                        break;
                    case "4":
                        await CancelSubscriptionAsync();
                        break;
                    case "5":
                        await RevokeSubscriptionAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Subscription Demo: {ex.Message}");
            }
        }

        private async Task ListSubscriptionsAsync()
        {
            WriteSubHeader("Listing Subscriptions");
            
            var subscriptions = await Client.ListSubscriptionsAsync(1, 20);
            Console.WriteLine($"Total Subscriptions: {subscriptions.Items.Count}");
            
            if (subscriptions.Items.Count == 0)
            {
                Console.WriteLine("No subscriptions found.");
                return;
            }
            
            foreach (var subscription in subscriptions.Items.Take(10))
            {
                Console.WriteLine($"\nSubscription ID: {subscription.Id}");
                Console.WriteLine($"  Status: {subscription.Status}");
                Console.WriteLine($"  Created: {subscription.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                
                // Note: StartedAt field is not available in the current model
                
                if (subscription.CurrentPeriodStart != null && subscription.CurrentPeriodEnd != null)
                {
                    Console.WriteLine($"  Current Period: {subscription.CurrentPeriodStart:yyyy-MM-dd} to {subscription.CurrentPeriodEnd:yyyy-MM-dd}");
                }
                
                // Note: CanceledAt, EndedAt, UserId fields are not available in the current model
                
                if (!string.IsNullOrEmpty(subscription.ProductId))
                {
                    Console.WriteLine($"  Product ID: {subscription.ProductId}");
                }
            }
            
            if (subscriptions.Items.Count > 10)
            {
                Console.WriteLine($"\n... and {subscriptions.Items.Count - 10} more subscriptions");
            }
        }

        private async Task GetSubscriptionDetailsAsync()
        {
            WriteSubHeader("Get Subscription Details");
            
            // List recent subscriptions to help user choose
            var subscriptions = await Client.ListSubscriptionsAsync(1, 5);
            if (subscriptions.Items.Count > 0)
            {
                Console.WriteLine("Recent subscriptions:");
                foreach (var s in subscriptions.Items)
                {
                    Console.WriteLine($"  {s.Id}: {s.Status} (Created: {s.CreatedAt:yyyy-MM-dd})");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter subscription ID: ");
            var subscriptionId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                Console.WriteLine("Subscription ID is required.");
                return;
            }
            
            var subscription = await Client.GetSubscriptionAsync(subscriptionId);
            
            Console.WriteLine($"\nSubscription Details:");
            Console.WriteLine($"ID: {subscription.Id}");
            Console.WriteLine($"Status: {subscription.Status}");
            Console.WriteLine($"Created: {subscription.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Modified: {subscription.ModifiedAt:yyyy-MM-dd HH:mm:ss}");
            
            // Note: StartedAt field is not available in the current model
            
            if (subscription.CurrentPeriodStart != null && subscription.CurrentPeriodEnd != null)
            {
                Console.WriteLine($"Current Period: {subscription.CurrentPeriodStart:yyyy-MM-dd} to {subscription.CurrentPeriodEnd:yyyy-MM-dd}");
            }
            
            // Note: CanceledAt, EndedAt, UserId, OrganizationId fields are not available in the current model
            
            if (!string.IsNullOrEmpty(subscription.ProductId))
            {
                Console.WriteLine($"Product ID: {subscription.ProductId}");
            }
            
            if (!string.IsNullOrEmpty(subscription.PriceId))
            {
                Console.WriteLine($"Price ID: {subscription.PriceId}");
            }
            
            // Note: CheckoutId field is not available in the current model
        }

        private async Task CreateSubscriptionAsync()
        {
            WriteSubHeader("Create Subscription");
            
            // First, list customers to choose from
            var customers = await Client.ListCustomersAsync(1, 10);
            if (customers.Items.Count == 0)
            {
                Console.WriteLine("No customers found. Please create a customer first.");
                return;
            }
            
            Console.WriteLine("Available customers:");
            foreach (var customer in customers.Items.Take(5))
            {
                Console.WriteLine($"  {customer.Id}: {customer.Email} ({customer.Name ?? "No name"})");
            }
            
            Console.Write("\nEnter customer ID: ");
            var customerId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(customerId))
            {
                Console.WriteLine("Customer ID is required.");
                return;
            }
            
            // Get price to use
            string? priceIdToUse = PriceId;
            
            if (string.IsNullOrWhiteSpace(priceIdToUse))
            {
                Console.WriteLine("\nNo default price configured. Fetching available prices...");
                
                if (!string.IsNullOrWhiteSpace(ProductId))
                {
                    var prices = await Client.ListPricesAsync(ProductId);
                    if (prices.Items.Count > 0)
                    {
                        Console.WriteLine("Available prices:");
                        for (int i = 0; i < Math.Min(5, prices.Items.Count); i++)
                        {
                            var price = prices.Items[i];
                            Console.WriteLine($"  {i + 1}. {price.Id}: {price.PriceAmount} {price.PriceCurrency} ({price.Type})");
                        }
                        
                        Console.Write("Select price number: ");
                        if (int.TryParse(Console.ReadLine(), out var priceChoice) && 
                            priceChoice > 0 && priceChoice <= prices.Items.Count)
                        {
                            priceIdToUse = prices.Items[priceChoice - 1].Id;
                        }
                    }
                }
            }
            
            if (string.IsNullOrWhiteSpace(priceIdToUse))
            {
                Console.Write("Enter price ID manually: ");
                priceIdToUse = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(priceIdToUse))
                {
                    Console.WriteLine("Price ID is required for subscription creation.");
                    return;
                }
            }
            
            Console.WriteLine($"\nCreating subscription:");
            Console.WriteLine($"  Customer: {customerId}");
            Console.WriteLine($"  Price: {priceIdToUse}");
            Console.Write("Confirm creation? (y/n): ");
            
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Subscription creation cancelled.");
                return;
            }
            
            var subscription = await Client.CreateSubscriptionAsync(customerId, priceIdToUse);
            
            Console.WriteLine($"\nSubscription created successfully!");
            Console.WriteLine($"Subscription ID: {subscription.Id}");
            Console.WriteLine($"Status: {subscription.Status}");
            
            if (subscription.CurrentPeriodStart != null && subscription.CurrentPeriodEnd != null)
            {
                Console.WriteLine($"Current Period: {subscription.CurrentPeriodStart:yyyy-MM-dd} to {subscription.CurrentPeriodEnd:yyyy-MM-dd}");
            }
        }

        private async Task CancelSubscriptionAsync()
        {
            WriteSubHeader("Cancel Subscription");
            
            // List active subscriptions
            var subscriptions = await Client.ListSubscriptionsAsync(1, 20);
            var activeSubscriptions = subscriptions.Items
                .Where(s => s.Status == "active" || s.Status == "trialing")
                .ToList();
            
            if (activeSubscriptions.Count == 0)
            {
                Console.WriteLine("No active subscriptions to cancel.");
                return;
            }
            
            Console.WriteLine("Active subscriptions:");
            foreach (var sub in activeSubscriptions.Take(5))
            {
                Console.WriteLine($"  {sub.Id}: {sub.Status}");
            }
            
            Console.Write("\nEnter subscription ID to cancel: ");
            var subscriptionId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                Console.WriteLine("Subscription ID is required.");
                return;
            }
            
            Console.WriteLine("\n⚠️ Canceling a subscription will stop it at the end of the current period.");
            Console.Write("Confirm cancellation? (y/n): ");
            
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Cancellation aborted.");
                return;
            }
            
            var success = await Client.CancelSubscriptionAsync(subscriptionId);
            
            if (success)
            {
                Console.WriteLine($"\nSubscription cancelled successfully!");
                Console.WriteLine($"Subscription ID: {subscriptionId}");
                Console.WriteLine("The subscription will end at the end of the current period.");
            }
            else
            {
                Console.WriteLine($"Failed to cancel subscription {subscriptionId}.");
            }
        }

        private async Task RevokeSubscriptionAsync()
        {
            WriteSubHeader("Revoke Subscription");
            
            Console.WriteLine("⚠️ WARNING: Revoking immediately terminates the subscription!");
            Console.WriteLine("Unlike cancellation, revocation takes effect immediately.\n");
            
            // List active subscriptions
            var subscriptions = await Client.ListSubscriptionsAsync(1, 20);
            var activeSubscriptions = subscriptions.Items
                .Where(s => s.Status == "active" || s.Status == "trialing")
                .ToList();
            
            if (activeSubscriptions.Count == 0)
            {
                Console.WriteLine("No active subscriptions to revoke.");
                return;
            }
            
            Console.WriteLine("Active subscriptions:");
            foreach (var sub in activeSubscriptions.Take(5))
            {
                Console.WriteLine($"  {sub.Id}: {sub.Status}");
            }
            
            Console.Write("\nEnter subscription ID to revoke: ");
            var subscriptionId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                Console.WriteLine("Subscription ID is required.");
                return;
            }
            
            Console.WriteLine("\n⚠️ This will IMMEDIATELY terminate the subscription!");
            Console.Write("Type 'REVOKE' to confirm: ");
            
            if (Console.ReadLine() != "REVOKE")
            {
                Console.WriteLine("Revocation cancelled.");
                return;
            }
            
            var success = await Client.RevokeSubscriptionAsync(subscriptionId);
            
            if (success)
            {
                Console.WriteLine($"\nSubscription revoked!");
                Console.WriteLine($"Subscription ID: {subscriptionId}");
                Console.WriteLine("The subscription has been immediately terminated.");
            }
            else
            {
                Console.WriteLine($"Failed to revoke subscription {subscriptionId}.");
            }
        }
    }
}