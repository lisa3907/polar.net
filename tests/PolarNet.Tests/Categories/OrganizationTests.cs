using System;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Organization API endpoints.
    /// </summary>
    public class OrganizationTests : TestBase
    {
        public OrganizationTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task GetOrganization_ShouldReturnOrganizationDetails()
        {
            // Arrange
            SkipIfNoClient(nameof(GetOrganization_ShouldReturnOrganizationDetails));
            LogSection("Get Organization Test");

            // Act
            Log("Fetching organization details...");
            var organization = await Client!.GetOrganizationAsync();

            // Assert
            Assert.NotNull(organization);
            Assert.NotEmpty(organization.Id);
            Assert.NotEmpty(organization.Name);
            Assert.NotEmpty(organization.Slug);
            
            Log($"✓ Organization retrieved: {organization.Name} (ID: {organization.Id})");
            Log($"  Slug: {organization.Slug}");
            Log($"  Created: {organization.CreatedAt:yyyy-MM-dd}");
            
            if (!string.IsNullOrEmpty(organization.Email))
            {
                Log($"  Email: {organization.Email}");
            }
            
            if (!string.IsNullOrEmpty(organization.Website))
            {
                Log($"  Website: {organization.Website}");
            }
        }

        [Fact]
        public async Task GetOrganization_ShouldHaveValidTimestamps()
        {
            // Arrange
            SkipIfNoClient(nameof(GetOrganization_ShouldHaveValidTimestamps));
            
            // Act
            var organization = await Client!.GetOrganizationAsync();

            // Assert
            Assert.NotNull(organization);
            Assert.True(organization.CreatedAt > DateTime.MinValue);
            Assert.True(organization.CreatedAt <= DateTime.UtcNow);
            Assert.True(organization.ModifiedAt >= organization.CreatedAt);
            Assert.True(organization.ModifiedAt <= DateTime.UtcNow);
            
            Log($"✓ Organization timestamps valid");
            Log($"  Created: {organization.CreatedAt:O}");
            Log($"  Modified: {organization.ModifiedAt:O}");
        }

        [Fact]
        public async Task GetOrganization_ShouldMatchConfiguredId()
        {
            // Arrange
            SkipIfNoClient(nameof(GetOrganization_ShouldMatchConfiguredId));
            Skip.If(string.IsNullOrEmpty(OrganizationId), "No OrganizationId configured");
            
            // Act
            var organization = await Client!.GetOrganizationAsync();

            // Assert
            Assert.NotNull(organization);
            Assert.Equal(OrganizationId, organization.Id);
            
            Log($"✓ Organization ID matches configuration: {organization.Id}");
        }
    }
}