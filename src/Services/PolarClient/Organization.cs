using System.Threading.Tasks;
using PolarNet.Models;
using System.Text.Json;
using System.Net.Http;
using System;
using System.Net;

namespace PolarNet.Services
{
    /// <summary>
    /// Organization-related endpoints.
    /// </summary>
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Retrieves the organization configured in <see cref="PolarClientOptions.OrganizationId"/>.
        /// </summary>
        /// <returns>The <see cref="PolarOrganization"/> resource.</returns>
        /// <exception cref="ArgumentException">Thrown when <c>OrganizationId</c> is not set in options.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarOrganization> GetOrganizationAsync()
        {
            var orgId = _organizationId ?? throw new ArgumentException("OrganizationId must be provided in options");
            using var response = await SendAsync(HttpMethod.Get, $"/v1/organizations/{orgId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarOrganization>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarOrganization");
        }
    }
}
