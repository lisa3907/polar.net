using Microsoft.Extensions.Configuration;
using PolarNet.Models.Resources;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Webhook Management API operations.
    /// </summary>
    public class WebhookDemo : DemoBase
    {
        public WebhookDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Webhook Management Demo");

            Console.WriteLine("Select a webhook operation:");
            Console.WriteLine("1. List all webhook endpoints");
            Console.WriteLine("2. Create new webhook endpoint");
            Console.WriteLine("3. Get webhook endpoint details");
            Console.WriteLine("4. Update webhook endpoint");
            Console.WriteLine("5. Test webhook endpoint");
            Console.WriteLine("6. Delete webhook endpoint");
            Console.Write("\nEnter choice [1-6]: ");
            
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListWebhooksAsync();
                        break;
                    case "2":
                        await CreateWebhookAsync();
                        break;
                    case "3":
                        await GetWebhookDetailsAsync();
                        break;
                    case "4":
                        await UpdateWebhookAsync();
                        break;
                    case "5":
                        await TestWebhookAsync();
                        break;
                    case "6":
                        await DeleteWebhookAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Webhook Demo: {ex.Message}");
            }
        }

        private async Task ListWebhooksAsync()
        {
            WriteSubHeader("Listing Webhook Endpoints");
            
            var webhooks = await Client.ListWebhookEndpointsAsync(1, 20);
            Console.WriteLine($"Total Webhook Endpoints: {webhooks.Items.Count}");
            
            if (webhooks.Items.Count == 0)
            {
                Console.WriteLine("No webhook endpoints configured.");
                return;
            }
            
            foreach (var webhook in webhooks.Items)
            {
                Console.WriteLine($"\nWebhook ID: {webhook.Id}");
                Console.WriteLine($"  URL: {webhook.Url}");
                Console.WriteLine($"  Format: {webhook.Format}");
                Console.WriteLine($"  Events: {string.Join(", ", webhook.Events.Take(3))}");
                if (webhook.Events.Count > 3)
                {
                    Console.WriteLine($"         ...and {webhook.Events.Count - 3} more events");
                }
                Console.WriteLine($"  Created: {webhook.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }
        }

        private async Task CreateWebhookAsync()
        {
            WriteSubHeader("Create Webhook Endpoint");
            
            Console.Write("Enter webhook URL: ");
            var url = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("URL is required.");
                return;
            }
            
            Console.WriteLine("\nAvailable event types:");
            var eventTypes = new[]
            {
                "checkout.created", "checkout.updated",
                "customer.created", "customer.updated", "customer.deleted",
                "order.created", "order.updated", "order.paid", "order.refunded",
                "subscription.created", "subscription.updated", "subscription.active",
                "subscription.canceled", "subscription.revoked",
                "refund.created", "refund.updated",
                "product.created", "product.updated",
                "benefit.created", "benefit.updated",
                "organization.updated"
            };
            
            for (int i = 0; i < eventTypes.Length; i++)
            {
                Console.WriteLine($"{i + 1,2}. {eventTypes[i]}");
            }
            
            Console.WriteLine("\nEnter event numbers to subscribe (comma-separated, e.g., 1,3,5): ");
            var eventChoices = Console.ReadLine();
            
            var selectedEvents = new List<string>();
            if (!string.IsNullOrWhiteSpace(eventChoices))
            {
                foreach (var choice in eventChoices.Split(','))
                {
                    if (int.TryParse(choice.Trim(), out var index) && index > 0 && index <= eventTypes.Length)
                    {
                        selectedEvents.Add(eventTypes[index - 1]);
                    }
                }
            }
            
            if (selectedEvents.Count == 0)
            {
                Console.WriteLine("No events selected. Using default: order.created");
                selectedEvents.Add("order.created");
            }
            
            Console.Write("Enter webhook secret (optional, press Enter to skip): ");
            var secret = Console.ReadLine();
            
            Console.WriteLine($"\nCreating webhook for: {url}");
            Console.WriteLine($"Events: {string.Join(", ", selectedEvents)}");
            Console.Write("Confirm creation? (y/n): ");
            
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Creation cancelled.");
                return;
            }
            
            var webhook = await Client.CreateWebhookEndpointAsync(
                url, 
                selectedEvents, 
                string.IsNullOrWhiteSpace(secret) ? null : secret
            );
            
            Console.WriteLine($"\nWebhook created successfully!");
            Console.WriteLine($"ID: {webhook.Id}");
            Console.WriteLine($"URL: {webhook.Url}");
            Console.WriteLine($"Format: {webhook.Format}");
        }

        private async Task GetWebhookDetailsAsync()
        {
            WriteSubHeader("Get Webhook Details");
            
            // List webhooks to help user choose
            var webhooks = await Client.ListWebhookEndpointsAsync(1, 5);
            if (webhooks.Items.Count > 0)
            {
                Console.WriteLine("Recent webhooks:");
                foreach (var w in webhooks.Items)
                {
                    Console.WriteLine($"  {w.Id}: {w.Url}");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter webhook ID: ");
            var webhookId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(webhookId))
            {
                Console.WriteLine("Webhook ID is required.");
                return;
            }
            
            var webhook = await Client.GetWebhookEndpointAsync(webhookId);
            
            Console.WriteLine($"\nWebhook Details:");
            Console.WriteLine($"ID: {webhook.Id}");
            Console.WriteLine($"URL: {webhook.Url}");
            Console.WriteLine($"Format: {webhook.Format}");
            Console.WriteLine($"Created: {webhook.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            if (webhook.ModifiedAt != null)
            {
                Console.WriteLine($"Modified: {webhook.ModifiedAt:yyyy-MM-dd HH:mm:ss}");
            }
            
            Console.WriteLine($"\nSubscribed Events ({webhook.Events.Count}):");
            foreach (var evt in webhook.Events)
            {
                Console.WriteLine($"  - {evt}");
            }
        }

        private async Task UpdateWebhookAsync()
        {
            WriteSubHeader("Update Webhook Endpoint");
            
            // List webhooks to help user choose
            var webhooks = await Client.ListWebhookEndpointsAsync(1, 5);
            if (webhooks.Items.Count > 0)
            {
                Console.WriteLine("Recent webhooks:");
                foreach (var w in webhooks.Items)
                {
                    Console.WriteLine($"  {w.Id}: {w.Url}");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter webhook ID to update: ");
            var webhookId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(webhookId))
            {
                Console.WriteLine("Webhook ID is required.");
                return;
            }
            
            // Get current webhook details
            var current = await Client.GetWebhookEndpointAsync(webhookId);
            Console.WriteLine($"\nCurrent URL: {current.Url}");
            Console.WriteLine($"Current Events: {string.Join(", ", current.Events)}");
            
            Console.WriteLine("\nWhat would you like to update?");
            Console.WriteLine("1. URL only");
            Console.WriteLine("2. Events only");
            Console.WriteLine("3. Both URL and Events");
            Console.Write("Choice [1-3]: ");
            
            var updateChoice = Console.ReadLine();
            var updateRequest = new UpdateWebhookEndpointRequest();
            
            if (updateChoice == "1" || updateChoice == "3")
            {
                Console.Write("Enter new URL (or press Enter to keep current): ");
                var newUrl = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newUrl))
                {
                    updateRequest.Url = newUrl;
                }
            }
            
            if (updateChoice == "2" || updateChoice == "3")
            {
                Console.WriteLine("\nAvailable event types:");
                var eventTypes = new[]
                {
                    "order.created", "order.updated", "order.paid",
                    "subscription.created", "subscription.updated", "subscription.canceled",
                    "customer.created", "customer.updated",
                    "refund.created", "refund.updated"
                };
                
                for (int i = 0; i < eventTypes.Length; i++)
                {
                    Console.WriteLine($"{i + 1,2}. {eventTypes[i]}");
                }
                
                Console.WriteLine("Enter event numbers (comma-separated): ");
                var eventChoices = Console.ReadLine();
                
                var newEvents = new List<string>();
                if (!string.IsNullOrWhiteSpace(eventChoices))
                {
                    foreach (var choice in eventChoices.Split(','))
                    {
                        if (int.TryParse(choice.Trim(), out var index) && index > 0 && index <= eventTypes.Length)
                        {
                            newEvents.Add(eventTypes[index - 1]);
                        }
                    }
                }
                
                if (newEvents.Count > 0)
                {
                    updateRequest.Events = newEvents;
                }
            }
            
            if (updateRequest.Url == null && updateRequest.Events == null)
            {
                Console.WriteLine("No changes specified.");
                return;
            }
            
            var updated = await Client.UpdateWebhookEndpointAsync(webhookId, updateRequest);
            
            Console.WriteLine($"\nWebhook updated successfully!");
            Console.WriteLine($"URL: {updated.Url}");
            Console.WriteLine($"Events: {string.Join(", ", updated.Events)}");
        }

        private async Task TestWebhookAsync()
        {
            WriteSubHeader("Test Webhook Endpoint");
            
            // List webhooks to help user choose
            var webhooks = await Client.ListWebhookEndpointsAsync(1, 5);
            if (webhooks.Items.Count > 0)
            {
                Console.WriteLine("Available webhooks:");
                foreach (var w in webhooks.Items)
                {
                    Console.WriteLine($"  {w.Id}: {w.Url}");
                }
                Console.WriteLine();
            }
            
            Console.Write("Enter webhook ID to test: ");
            var webhookId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(webhookId))
            {
                Console.WriteLine("Webhook ID is required.");
                return;
            }
            
            Console.WriteLine("\nSending test event to webhook...");
            
            var success = await Client.TestWebhookEndpointAsync(webhookId);
            
            if (success)
            {
                Console.WriteLine("✅ Test event sent successfully!");
                Console.WriteLine("Check your webhook endpoint to verify receipt.");
            }
            else
            {
                Console.WriteLine("❌ Test failed. This could mean:");
                Console.WriteLine("  - The webhook URL is not reachable");
                Console.WriteLine("  - The endpoint returned an error");
                Console.WriteLine("  - The URL doesn't exist");
            }
        }

        private async Task DeleteWebhookAsync()
        {
            WriteSubHeader("Delete Webhook Endpoint");
            
            // List webhooks to help user choose
            var webhooks = await Client.ListWebhookEndpointsAsync(1, 10);
            if (webhooks.Items.Count > 0)
            {
                Console.WriteLine("Current webhooks:");
                foreach (var w in webhooks.Items)
                {
                    Console.WriteLine($"  {w.Id}: {w.Url}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("No webhooks to delete.");
                return;
            }
            
            Console.Write("Enter webhook ID to delete: ");
            var webhookId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(webhookId))
            {
                Console.WriteLine("Webhook ID is required.");
                return;
            }
            
            Console.Write($"Are you sure you want to delete webhook {webhookId}? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }
            
            var success = await Client.DeleteWebhookEndpointAsync(webhookId);
            Console.WriteLine(success ? "✅ Webhook deleted successfully." : "❌ Failed to delete webhook.");
        }
    }
}