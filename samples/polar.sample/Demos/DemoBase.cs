using Microsoft.Extensions.Configuration;
using PolarNet.Services;

namespace PolarSample.Demos
{
    /// <summary>
    /// Base class for all demo implementations.
    /// </summary>
    public abstract class DemoBase
    {
        protected readonly PolarClient Client;
        protected readonly IConfigurationRoot Config;
        protected readonly string? OrganizationId;
        protected readonly string? ProductId;
        protected readonly string? PriceId;

        protected DemoBase(IConfigurationRoot config)
        {
            Config = config;
            
            var polar = config.GetSection("PolarSettings");
            var accessToken = polar["AccessToken"] ?? string.Empty;
            OrganizationId = polar["OrganizationId"];
            ProductId = polar["ProductId"];
            PriceId = polar["PriceId"];
            var useSandbox = string.Equals(polar["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
            var baseUrl = useSandbox ? polar["SandboxApiUrl"] : polar["ProductionApiUrl"];

            Client = new PolarClient(new PolarClientOptions
            {
                AccessToken = accessToken,
                BaseUrl = baseUrl ?? "",
                OrganizationId = OrganizationId,
                DefaultProductId = ProductId,
                DefaultPriceId = PriceId,
            });
        }

        public abstract Task RunAsync();
        
        protected void WriteHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine();
        }

        protected void WriteSubHeader(string title)
        {
            Console.WriteLine($"\n--- {title} ---");
        }
    }
}