using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Customer API operations.
    /// </summary>
    public class CustomerDemo : DemoBase
    {
        public CustomerDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Customer Management Demo");

            Console.WriteLine("Select a customer operation:");
            Console.WriteLine("1. List all customers");
            Console.WriteLine("2. Create a new customer");
            Console.WriteLine("3. Get customer details");
            Console.WriteLine("4. Delete a specific customer");
            Console.WriteLine("5. Delete ALL customers (CAUTION!)");
            Console.WriteLine("6. Get customer state");
            Console.Write("\nEnter choice [1-6]: ");
            
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListCustomersAsync();
                        break;
                    case "2":
                        await CreateCustomerAsync();
                        break;
                    case "3":
                        await GetCustomerDetailsAsync();
                        break;
                    case "4":
                        await DeleteCustomerAsync();
                        break;
                    case "5":
                        await DeleteAllCustomersAsync();
                        break;
                    case "6":
                        await GetCustomerStateAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Customer Demo: {ex.Message}");
            }
        }

        private async Task ListCustomersAsync()
        {
            WriteSubHeader("Listing Customers");
            
            var customers = await Client.ListCustomersAsync(1, 100);
            Console.WriteLine($"Total Customers: {customers.Items.Count}");
            
            foreach (var customer in customers.Items.Take(10)) // Show first 10
            {
                Console.WriteLine($"- {customer.Id}: {customer.Email} ({customer.Name ?? "No name"})");
                Console.WriteLine($"  Created: {customer.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }
            
            if (customers.Items.Count > 10)
            {
                Console.WriteLine($"... and {customers.Items.Count - 10} more customers");
            }
        }

        private async Task CreateCustomerAsync()
        {
            WriteSubHeader("Create New Customer");
            
            Console.Write("Enter customer email: ");
            var email = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email is required.");
                return;
            }
            
            Console.Write("Enter customer name (optional): ");
            var name = Console.ReadLine();
            
            var customer = await Client.CreateCustomerAsync(
                email, 
                string.IsNullOrWhiteSpace(name) ? null : name
            );
            
            Console.WriteLine($"\nCustomer created successfully!");
            Console.WriteLine($"ID: {customer.Id}");
            Console.WriteLine($"Email: {customer.Email}");
            Console.WriteLine($"Name: {customer.Name ?? "Not provided"}");
        }

        private async Task GetCustomerDetailsAsync()
        {
            WriteSubHeader("Get Customer Details");
            
            Console.Write("Enter customer ID: ");
            var customerId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(customerId))
            {
                Console.WriteLine("Customer ID is required.");
                return;
            }
            
            var customer = await Client.GetCustomerAsync(customerId);
            
            Console.WriteLine($"\nCustomer Details:");
            Console.WriteLine($"ID: {customer.Id}");
            Console.WriteLine($"Email: {customer.Email}");
            Console.WriteLine($"Name: {customer.Name ?? "Not provided"}");
            Console.WriteLine($"Created: {customer.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Modified: {customer.ModifiedAt:yyyy-MM-dd HH:mm:ss}");
            
            // Note: Metadata field is not available in the current model
        }

        private async Task DeleteCustomerAsync()
        {
            WriteSubHeader("Delete Customer");
            
            Console.Write("Enter customer ID to delete: ");
            var customerId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(customerId))
            {
                Console.WriteLine("Customer ID is required.");
                return;
            }
            
            Console.Write($"Are you sure you want to delete customer {customerId}? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }
            
            bool success = await Client.DeleteCustomerAsync(customerId);
            Console.WriteLine(success ? "Customer deleted successfully." : "Failed to delete customer.");
        }

        private async Task DeleteAllCustomersAsync()
        {
            WriteSubHeader("Delete ALL Customers");
            
            Console.WriteLine("WARNING: This will delete ALL customers!");
            Console.Write("Type 'DELETE ALL' to confirm: ");
            
            if (Console.ReadLine() != "DELETE ALL")
            {
                Console.WriteLine("Operation cancelled.");
                return;
            }
            
            Console.WriteLine("\nDeleting all customers...");
            int page = 1, limit = 100, totalDeleted = 0;
            
            while (true)
            {
                var customers = await Client.ListCustomersAsync(page, limit);
                if (customers.Items.Count == 0) break;
                
                foreach (var customer in customers.Items)
                {
                    Console.Write($"Deleting {customer.Id} ({customer.Email})... ");
                    bool success = await Client.DeleteCustomerAsync(customer.Id);
                    Console.WriteLine(success ? "OK" : "FAILED");
                    if (success) totalDeleted++;
                }
                
                if (customers.Items.Count < limit) break;
                page++;
            }
            
            Console.WriteLine($"\nTotal customers deleted: {totalDeleted}");
        }

        private async Task GetCustomerStateAsync()
        {
            WriteSubHeader("Get Customer State");
            
            Console.Write("Enter customer ID: ");
            var customerId = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(customerId))
            {
                Console.WriteLine("Customer ID is required.");
                return;
            }
            
            var state = await Client.GetCustomerStateAsync(customerId);
            
            Console.WriteLine($"\nCustomer State:");
            Console.WriteLine($"Customer ID: {customerId}");
            
            // Display active subscriptions
            if (state.ActiveSubscriptions != null && state.ActiveSubscriptions.Count > 0)
            {
                Console.WriteLine($"Active Subscriptions: {state.ActiveSubscriptions.Count}");
                // Note: PolarCustomerStateSubscription structure may vary
            }
            else
            {
                Console.WriteLine("No active subscriptions");
            }
            
            // Display granted benefits if available
            if (state.GrantedBenefits != null && state.GrantedBenefits.Count > 0)
            {
                Console.WriteLine($"Granted Benefits: {state.GrantedBenefits.Count}");
                // Note: PolarCustomerStateGrantedBenefit structure may vary
            }
            
            // Display active meters if available
            if (state.ActiveMeters != null && state.ActiveMeters.Count > 0)
            {
                Console.WriteLine($"Active Meters: {state.ActiveMeters.Count}");
                // Note: PolarCustomerStateActiveMeter structure may vary
            }
        }
    }
}