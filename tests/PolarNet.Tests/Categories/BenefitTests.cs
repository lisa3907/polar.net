using System;
using System.Linq;
using System.Threading.Tasks;
using PolarNet.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace PolarNet.Tests.Categories
{
    /// <summary>
    /// Tests for Benefit API endpoints.
    /// </summary>
    public class BenefitTests : TestBase
    {
        public BenefitTests(PolarSandboxFixture fixture, ITestOutputHelper output) 
            : base(fixture, output)
        {
        }

        [Fact]
        public async Task ListBenefits_ShouldReturnBenefits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListBenefits_ShouldReturnBenefits));
            LogSection("List Benefits Test");

            // Act
            Log("Fetching benefits...");
            var benefits = await Client!.ListBenefitsAsync(1, 10);

            // Assert
            Assert.NotNull(benefits);
            Assert.NotNull(benefits.Items);
            
            if (benefits.Items.Count > 0)
            {
                Log($"✓ Found {benefits.Items.Count} benefits");
                
                foreach (var benefit in benefits.Items.Take(3))
                {
                    Assert.NotEmpty(benefit.Id);
                    Assert.NotEmpty(benefit.Name);
                    Assert.NotEmpty(benefit.Type);
                    
                    Log($"  - Benefit: {benefit.Name} (ID: {benefit.Id})");
                    Log($"    Type: {benefit.Type}");
                    
                    if (!string.IsNullOrEmpty(benefit.Description))
                    {
                        Log($"    Description: {benefit.Description}");
                    }
                    
                    if (!string.IsNullOrEmpty(benefit.OrganizationId))
                    {
                        Log($"    Organization: {benefit.OrganizationId}");
                    }
                }
            }
            else
            {
                Log("⚠ No benefits found (this may be expected for a new organization)");
            }
        }

        [Fact]
        public async Task ListBenefits_WithPagination_ShouldRespectLimits()
        {
            // Arrange
            SkipIfNoClient(nameof(ListBenefits_WithPagination_ShouldRespectLimits));
            const int requestedLimit = 5;
            
            // Act
            var benefits = await Client!.ListBenefitsAsync(1, requestedLimit);

            // Assert
            Assert.NotNull(benefits);
            Assert.NotNull(benefits.Items);
            Assert.True(benefits.Items.Count <= requestedLimit, 
                $"Expected at most {requestedLimit} items, but got {benefits.Items.Count}");
            
            Log($"✓ Pagination respected: requested {requestedLimit}, received {benefits.Items.Count}");
        }

        [Fact]
        public async Task ListBenefits_ShouldHaveValidBenefitTypes()
        {
            // Arrange
            SkipIfNoClient(nameof(ListBenefits_ShouldHaveValidBenefitTypes));
            
            // Act
            var benefits = await Client!.ListBenefitsAsync(1, 20);

            // Assert
            Assert.NotNull(benefits);
            
            if (benefits.Items.Count > 0)
            {
                var validTypes = new[] { "custom", "articles", "ads", "discord", "github_repository", "downloadables" };
                
                foreach (var benefit in benefits.Items)
                {
                    Assert.Contains(benefit.Type, validTypes);
                }
                
                Log($"✓ All {benefits.Items.Count} benefits have valid types");
                
                // Group by type for summary
                var typeGroups = benefits.Items.GroupBy(b => b.Type);
                foreach (var group in typeGroups)
                {
                    Log($"  {group.Key}: {group.Count()} benefit(s)");
                }
            }
        }

        [Fact]
        public async Task ListBenefits_ShouldMatchOrganization()
        {
            // Arrange
            SkipIfNoClient(nameof(ListBenefits_ShouldMatchOrganization));
            Skip.If(string.IsNullOrEmpty(OrganizationId), "No OrganizationId configured");
            
            // Act
            var benefits = await Client!.ListBenefitsAsync(1, 10);

            // Assert
            Assert.NotNull(benefits);
            
            if (benefits.Items.Count > 0)
            {
                foreach (var benefit in benefits.Items.Where(b => !string.IsNullOrEmpty(b.OrganizationId)))
                {
                    Assert.Equal(OrganizationId, benefit.OrganizationId);
                }
                
                Log($"✓ All benefits belong to organization: {OrganizationId}");
            }
        }

        [Fact]
        public async Task ListBenefits_MultiplePage_ShouldWork()
        {
            // Arrange
            SkipIfNoClient(nameof(ListBenefits_MultiplePage_ShouldWork));
            LogSection("Multiple Page Benefits Test");
            
            // Act
            Log("Fetching page 1...");
            var page1 = await Client!.ListBenefitsAsync(1, 5);
            
            Log("Fetching page 2...");
            var page2 = await Client!.ListBenefitsAsync(2, 5);

            // Assert
            Assert.NotNull(page1);
            Assert.NotNull(page2);
            Assert.NotNull(page1.Items);
            Assert.NotNull(page2.Items);
            
            // If there are items on page 2, they should be different from page 1
            if (page1.Items.Count > 0 && page2.Items.Count > 0)
            {
                var page1Ids = page1.Items.Select(b => b.Id).ToHashSet();
                var page2Ids = page2.Items.Select(b => b.Id).ToHashSet();
                
                Assert.Empty(page1Ids.Intersect(page2Ids));
                Log($"✓ Page 1 and Page 2 have different benefits");
            }
            
            Log($"  Page 1: {page1.Items.Count} items");
            Log($"  Page 2: {page2.Items.Count} items");
        }
    }
}