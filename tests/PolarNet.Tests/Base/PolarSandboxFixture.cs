using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Services;
using Xunit;

namespace PolarNet.Tests.Base
{
    /// <summary>
    /// Loads sandbox configuration from tests/appsettings.json and constructs a PolarClient for integration tests.
    /// If any required config is missing, tests will be skipped with a clear message.
    /// </summary>
    public sealed class PolarSandboxFixture : IAsyncLifetime
    {
        public PolarClient? Client { get; private set; }
        public string? OrganizationId { get; private set; }
        public string? ProductId { get; private set; }
        public string? PriceId { get; private set; }
        public string? WebhookBaseUrl { get; private set; }

        private IConfigurationRoot? _config;

        public Task InitializeAsync()
        {
            _config = new ConfigurationBuilder()
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true)
#else
                .AddJsonFile("appsettings.json", optional: false)
#endif
                .AddEnvironmentVariables(prefix: "POLAR_TEST_")
                .Build();

            var section = _config.GetSection("PolarSettings");
            var accessToken = section["AccessToken"] ?? string.Empty;
            var useSandbox = string.Equals(section["UseSandbox"], "true", StringComparison.OrdinalIgnoreCase);
            var baseUrl = (useSandbox ? section["SandboxApiUrl"] : section["ProductionApiUrl"]) ?? string.Empty;

            OrganizationId = section["OrganizationId"];
            ProductId = section["ProductId"];
            PriceId = section["PriceId"];
            WebhookBaseUrl = section["WebhookBaseUrl"];

            if (IsPlaceholder(accessToken) || string.IsNullOrWhiteSpace(baseUrl))
            {
                // Leave Client = null; tests will skip with message.
                return Task.CompletedTask;
            }

            var options = new PolarClientOptions
            {
                AccessToken = accessToken,
                BaseUrl = baseUrl,
                OrganizationId = !IsPlaceholder(OrganizationId) ? OrganizationId : null,
                DefaultProductId = !IsPlaceholder(ProductId) ? ProductId : null,
                DefaultPriceId = !IsPlaceholder(PriceId) ? PriceId : null
            };

            Client = new PolarClient(options);
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Client?.Dispose();
            return Task.CompletedTask;
        }

        internal static bool IsPlaceholder(string? value)
            => string.IsNullOrWhiteSpace(value) || value!.StartsWith("<", StringComparison.Ordinal);
    }
}