using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;

namespace PolarNet.Services
{
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Lists benefits for the configured organization.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of benefits.</returns>
        /// <exception cref="ArgumentException">Thrown if <c>OrganizationId</c> is not configured.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarBenefit>> ListBenefitsAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/benefits?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarBenefit>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize benefit list");
        }
    }
}