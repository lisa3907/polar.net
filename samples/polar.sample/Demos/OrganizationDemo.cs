using Microsoft.Extensions.Configuration;

namespace PolarSample.Demos
{
    /// <summary>
    /// Demonstrates Organization API operations.
    /// </summary>
    public class OrganizationDemo : DemoBase
    {
        public OrganizationDemo(IConfigurationRoot config) : base(config) { }

        public override async Task RunAsync()
        {
            WriteHeader("Organization Information");

            try
            {
                if (string.IsNullOrWhiteSpace(OrganizationId))
                {
                    Console.WriteLine("Organization ID is not configured in appsettings.json");
                    return;
                }

                WriteSubHeader("Fetching Organization Details");
                Console.WriteLine($"Organization ID: {OrganizationId}");
                
                var organization = await Client.GetOrganizationAsync();
                
                Console.WriteLine($"\nOrganization Details:");
                Console.WriteLine($"ID: {organization.Id}");
                Console.WriteLine($"Name: {organization.Name}");
                Console.WriteLine($"Slug: {organization.Slug}");
                
                if (!string.IsNullOrEmpty(organization.Email))
                {
                    Console.WriteLine($"Email: {organization.Email}");
                }
                
                if (!string.IsNullOrEmpty(organization.Website))
                {
                    Console.WriteLine($"Website: {organization.Website}");
                }
                
                // Note: Bio, Blog, Company, Location, TwitterUsername, AvatarUrl fields are not available in the current model
                
                Console.WriteLine($"Created: {organization.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"Modified: {organization.ModifiedAt:yyyy-MM-dd HH:mm:ss}");
                
                // Note: FeatureSettings, BillingEmail, DonationsEnabled, DefaultUpfrontSplitToContributors fields are not available in the current model
                
                Console.WriteLine("\n📌 Note: Organization settings are primarily managed through the Polar dashboard.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Organization Demo: {ex.Message}");
            }
        }
    }
}