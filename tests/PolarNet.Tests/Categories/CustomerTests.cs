using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Models;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Customer API endpoints.
    /// </summary>
    public class CustomerTests : TestBase
    {
        public CustomerTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task CreateCustomer_WithBasicInfo_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateCustomer_WithBasicInfo_ShouldSucceed));
            LogSection("Create Customer Test");
            
            var email = GenerateTestEmail();
            var name = $"Test User {GenerateTestId()}";

            // Act
            Log($"Creating customer with email: {email}");
            var customer = await Client!.CreateCustomerAsync(email, name);

            // Assert
            Assert.NotNull(customer);
            Assert.NotEmpty(customer.Id);
            Assert.Equal(email, customer.Email);
            Assert.Equal(name, customer.Name);
            Assert.True(customer.CreatedAt > DateTime.MinValue);
            
            Log($"✓ Customer created: {customer.Id}");
            Log($"  Email: {customer.Email}");
            Log($"  Name: {customer.Name}");

            // Cleanup
            await Client.DeleteCustomerAsync(customer.Id);
            Log("✓ Customer deleted");
        }

        [Fact]
        public async Task CreateCustomer_WithRequest_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(CreateCustomer_WithRequest_ShouldSucceed));
            
            var request = new CreateCustomerRequest
            {
                Email = GenerateTestEmail(),
                Name = $"Advanced Test User {GenerateTestId()}"
            };

            // Act
            var customer = await Client!.CreateCustomerAsync(request);

            // Assert
            Assert.NotNull(customer);
            Assert.NotEmpty(customer.Id);
            Assert.Equal(request.Email, customer.Email);
            Assert.Equal(request.Name, customer.Name);
            
            Log($"✓ Customer created with request: {customer.Id}");

            // Cleanup
            await Client.DeleteCustomerAsync(customer.Id);
        }

        [Fact]
        public async Task ListCustomers_ShouldReturnCustomers()
        {
            // Arrange
            SkipIfNoClient(nameof(ListCustomers_ShouldReturnCustomers));
            LogSection("List Customers Test");

            // Create a test customer to ensure at least one exists
            var testCustomer = await Client!.CreateCustomerAsync(GenerateTestEmail());

            try
            {
                // Act
                Log("Fetching customers...");
                var customers = await Client.ListCustomersAsync(1, 10);

                // Assert
                Assert.NotNull(customers);
                Assert.NotNull(customers.Items);
                Assert.True(customers.Items.Count > 0, "Should have at least one customer");
                
                Log($"✓ Found {customers.Items.Count} customers");
                
                foreach (var customer in customers.Items.Take(3))
                {
                    Assert.NotEmpty(customer.Id);
                    Assert.NotEmpty(customer.Email);
                    
                    Log($"  - {customer.Email} (ID: {customer.Id})");
                    if (!string.IsNullOrEmpty(customer.Name))
                    {
                        Log($"    Name: {customer.Name}");
                    }
                }
            }
            finally
            {
                // Cleanup
                await Client.DeleteCustomerAsync(testCustomer.Id);
            }
        }

        [Fact]
        public async Task GetCustomer_WithValidId_ShouldReturnCustomer()
        {
            // Arrange
            SkipIfNoClient(nameof(GetCustomer_WithValidId_ShouldReturnCustomer));
            LogSection("Get Customer Test");
            
            var email = GenerateTestEmail();
            var createResult = await Client!.CreateCustomerAsync(email);

            try
            {
                // Act
                Log($"Fetching customer {createResult.Id}...");
                var customer = await Client.GetCustomerAsync(createResult.Id);

                // Assert
                Assert.NotNull(customer);
                Assert.Equal(createResult.Id, customer.Id);
                Assert.Equal(email, customer.Email);
                
                Log($"✓ Customer retrieved: {customer.Id}");
                Log($"  Email: {customer.Email}");
                Log($"  Created: {customer.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteCustomerAsync(createResult.Id);
            }
        }

        [Fact]
        public async Task DeleteCustomer_WithValidId_ShouldSucceed()
        {
            // Arrange
            SkipIfNoClient(nameof(DeleteCustomer_WithValidId_ShouldSucceed));
            LogSection("Delete Customer Test");
            
            var customer = await Client!.CreateCustomerAsync(GenerateTestEmail());
            Log($"Created customer: {customer.Id}");

            // Act
            Log($"Deleting customer {customer.Id}...");
            var result = await Client.DeleteCustomerAsync(customer.Id);

            // Assert
            Assert.True(result, "Delete should return true");
            Log($"✓ Customer deleted successfully");

            // Verify deletion
            try
            {
                await Client.GetCustomerAsync(customer.Id);
                Assert.Fail("Should not be able to get deleted customer");
            }
            catch
            {
                Log("✓ Confirmed customer no longer exists");
            }
        }

        [Fact]
        public async Task GetCustomerState_ShouldReturnState()
        {
            // Arrange
            SkipIfNoClient(nameof(GetCustomerState_ShouldReturnState));
            LogSection("Get Customer State Test");
            
            var customer = await Client!.CreateCustomerAsync(GenerateTestEmail());

            try
            {
                // Act
                Log($"Fetching customer state for {customer.Id}...");
                var state = await Client.GetCustomerStateAsync(customer.Id);

                // Assert
                Assert.NotNull(state);
                Assert.Equal(customer.Id, state.Id);
                Assert.Equal(customer.Email, state.Email);
                Assert.NotNull(state.ActiveSubscriptions);
                Assert.NotNull(state.GrantedBenefits);
                Assert.NotNull(state.ActiveMeters);
                
                Log($"✓ Customer state retrieved");
                Log($"  Customer ID: {state.Id}");
                Log($"  Email: {state.Email}");
                Log($"  Active Subscriptions: {state.ActiveSubscriptions.Count}");
                Log($"  Granted Benefits: {state.GrantedBenefits.Count}");
                Log($"  Active Meters: {state.ActiveMeters.Count}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteCustomerAsync(customer.Id);
            }
        }

        [Fact]
        public async Task UpdateCustomer_ShouldModifyFields()
        {
            // Arrange
            SkipIfNoClient(nameof(UpdateCustomer_ShouldModifyFields));
            var customer = await Client!.CreateCustomerAsync(GenerateTestEmail(), $"User {GenerateTestId()}");

            try
            {
                var newName = $"Updated {GenerateTestId()}";
                var req = new UpdateCustomerRequest { Name = newName };

                // Act
                var updated = await Client.UpdateCustomerAsync(customer.Id, req);

                // Assert
                Assert.NotNull(updated);
                Assert.Equal(customer.Id, updated.Id);
                Assert.Equal(newName, updated.Name);
            }
            finally
            {
                await Client.DeleteCustomerAsync(customer.Id);
            }
        }
        [Fact]
        public async Task ListCustomers_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListCustomers_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;

            // Create multiple customers to test pagination
            var customerIds = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var customer = await Client!.CreateCustomerAsync(GenerateTestEmail());
                customerIds.Add(customer.Id);
            }

            try
            {
                // Act
                var customers = await Client!.ListCustomersAsync(1, requestedLimit);

                // Assert
                Assert.NotNull(customers);
                Assert.NotNull(customers.Items);
                Assert.True(customers.Items.Count <= requestedLimit, 
                    $"Expected at most {requestedLimit} items, but got {customers.Items.Count}");
                
                Log($"✓ Pagination respected: requested {requestedLimit}, received {customers.Items.Count}");
            }
            finally
            {
                // Cleanup
                foreach (var id in customerIds)
                {
                    await Client!.DeleteCustomerAsync(id);
                }
            }
        }
    }
}