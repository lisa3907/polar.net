using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Benefit API operations.
    /// </summary>
    public class BenefitDemo : DemoBase
    {
        public BenefitDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Benefits Demo");

            try
            {
                WriteSubHeader("Listing Benefits");
                
                var benefits = await Client.ListBenefitsAsync(1, 20);
                Console.WriteLine($"Total Benefits: {benefits.Items.Count}");
                
                if (benefits.Items.Count == 0)
                {
                    Console.WriteLine("No benefits found.");
                    Console.WriteLine("\nBenefits are typically created through the Polar dashboard.");
                    Console.WriteLine("They represent perks or features granted to customers.");
                    return;
                }
                
                foreach (var benefit in benefits.Items)
                {
                    Console.WriteLine($"\nBenefit ID: {benefit.Id}");
                    Console.WriteLine($"  Name: {benefit.Name}");
                    Console.WriteLine($"  Type: {benefit.Type}");
                    
                    if (!string.IsNullOrEmpty(benefit.Description))
                    {
                        Console.WriteLine($"  Description: {benefit.Description}");
                    }
                    
                    // Note: Deletable and CreatedAt fields are not available in the current model
                    
                    if (!string.IsNullOrEmpty(benefit.OrganizationId))
                    {
                        Console.WriteLine($"  Organization: {benefit.OrganizationId}");
                    }
                    
                    // Note: Properties field is not available in the current model
                }
                
                Console.WriteLine("\n📌 Note: Benefits are typically managed through the Polar dashboard.");
                Console.WriteLine("They can be granted to customers as part of subscriptions or purchases.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Benefits Demo: {ex.Message}");
            }
        }
    }
}