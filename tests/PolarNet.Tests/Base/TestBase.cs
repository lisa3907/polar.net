#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PolarNet.Services;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Base
{
    /// <summary>
    /// Base class for all Polar API tests providing common functionality.
    /// </summary>
    public abstract class TestBase : IClassFixture<PolarSandboxFixture>
    {
        protected readonly PolarSandboxFixture Fixture;
        protected readonly ITestOutputHelper Output;
        protected readonly PolarClient? Client;
        protected readonly string? OrganizationId;
        protected readonly string? ProductId;
        protected readonly string? PriceId;
        protected readonly string? WebhookBaseUrl;

        protected TestBase(PolarSandboxFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Client = fixture.Client;
            OrganizationId = fixture.OrganizationId;
            ProductId = fixture.ProductId;
            PriceId = fixture.PriceId;
            // Ensure WebhookBaseUrl always has a value for tests and is properly formatted
            WebhookBaseUrl = !string.IsNullOrEmpty(fixture.WebhookBaseUrl) && !fixture.WebhookBaseUrl.StartsWith("<") 
                ? fixture.WebhookBaseUrl.TrimEnd('/') 
                : "https://webhook.site";
        }

        /// <summary>
        /// Checks if the test should be skipped due to missing configuration.
        /// </summary>
        protected void SkipIfNoClient(string testName)
        {
            if (Client == null)
            {
                Output.WriteLine($"Skipping {testName}: No PolarClient configured. Check appsettings.json.");
                Skip.If(true, "No PolarClient configured. Set AccessToken in appsettings.json.");
            }
        }

        /// <summary>
        /// Generates a unique test identifier.
        /// </summary>
        protected string GenerateTestId(string prefix = "test")
        {
            return $"{prefix}_{Guid.NewGuid():N}".Substring(0, 20);
        }

        /// <summary>
        /// Generates a unique test email.
        /// </summary>
        protected string GenerateTestEmail()
        {
            // Use gmail.com instead of example.com as Polar API validates email domains
            return $"test_{Guid.NewGuid():N}@gmail.com";
        }

        /// <summary>
        /// Logs test output with timestamp.
        /// </summary>
        protected void Log(string message)
        {
            Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
        }

        /// <summary>
        /// Logs test section header.
        /// </summary>
        protected void LogSection(string section)
        {
            Output.WriteLine($"\n=== {section} ===");
        }

        /// <summary>
        /// Asserts that a task completes within the specified timeout.
        /// </summary>
        protected async Task AssertCompletesWithinAsync(Task task, TimeSpan timeout, string operation)
        {
            var timeoutTask = Task.Delay(timeout);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"{operation} did not complete within {timeout.TotalSeconds} seconds");
            }
            
            await task; // Propagate any exceptions
        }

        /// <summary>
        /// Retries an operation with exponential backoff.
        /// </summary>
        protected async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxAttempts = 3, int delayMs = 1000)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Log($"Attempt {attempt} failed: {ex.Message}. Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2; // Exponential backoff
                }
            }
            
            return await operation(); // Final attempt, let exception propagate
        }

        /// <summary>
        /// Generates a webhook URL using the configured WebhookBaseUrl.
        /// </summary>
        protected string GenerateWebhookUrl(string path = "webhook")
        {
            // WebhookBaseUrl is guaranteed to have a value and be trimmed from constructor
            return $"{WebhookBaseUrl}/{path}/{GenerateTestId()}";
        }
    }
}