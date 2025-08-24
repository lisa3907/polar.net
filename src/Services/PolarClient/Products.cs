#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System.Threading.Tasks;
using PolarNet.Models;
using System.Text.Json;
using System.Net.Http;
using System;
using System.Text;

namespace PolarNet.Services
{
    /// <summary>
    /// Product and price endpoints.
    /// </summary>
    public sealed partial class PolarClient
    {
        /// <summary>
        /// Retrieves a product by id, or uses <see cref="PolarClientOptions.DefaultProductId"/> when <paramref name="productId"/> is not provided.
        /// </summary>
        /// <param name="productId">Optional product id; falls back to <c>DefaultProductId</c> when null.</param>
        /// <returns>The <see cref="PolarProduct"/> resource.</returns>
        /// <exception cref="ArgumentException">Thrown when neither <paramref name="productId"/> nor <c>DefaultProductId</c> is available.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarProduct> GetProductAsync(string? productId = null)
        {
            var pid = productId ?? _defaultProductId ?? throw new ArgumentException("ProductId must be supplied or DefaultProductId set in options");
            using var response = await SendAsync(HttpMethod.Get, $"/v1/products/{pid}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarProduct>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarProduct");
        }

        /// <summary>
        /// Lists products with pagination.
        /// </summary>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of products.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarProduct>> ListProductsAsync(int page = 1, int limit = 10)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/products?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarProduct>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize product list");
        }

        /// <summary>
        /// Retrieves a price by id.
        /// </summary>
        /// <param name="priceId">Price identifier.</param>
        /// <returns>The <see cref="PolarPrice"/> resource.</returns>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarPrice> GetPriceAsync(string priceId)
        {
            using var response = await SendAsync(HttpMethod.Get, $"/v1/prices/{priceId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarPrice>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarPrice");
        }

        /// <summary>
        /// Lists prices for a product with pagination.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        /// <param name="page">1-based page number. Defaults to 1.</param>
        /// <param name="limit">Items per page (1-100). Defaults to 10.</param>
        /// <returns>Paged list of prices.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="productId"/> is empty.</exception>
        /// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
        public async Task<PolarListResponse<PolarPrice>> ListPricesAsync(string productId, int page = 1, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("productId is required", nameof(productId));
            using var response = await SendAsync(HttpMethod.Get, $"/v1/products/{productId}/prices?limit={limit}&page={page}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarListResponse<PolarPrice>>(json)
                   ?? throw new InvalidOperationException("Failed to deserialize price list");
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        public async Task<PolarProduct> CreateProductAsync(CreateProductRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Name is required", nameof(request.Name));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await SendAsync(HttpMethod.Post, "/v1/products", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create product: {response.StatusCode} - {error}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarProduct>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarProduct");
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        public async Task<PolarProduct> UpdateProductAsync(string productId, UpdateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("productId is required", nameof(productId));
            if (request is null) throw new ArgumentNullException(nameof(request));

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await SendAsync(new HttpMethod("PATCH"), $"/v1/products/{productId}", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update product: {response.StatusCode} - {error}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PolarProduct>(responseContent)
                   ?? throw new InvalidOperationException("Failed to deserialize PolarProduct");
        }

        /// <summary>
        /// Deletes a product by id.
        /// </summary>
        public async Task<bool> DeleteProductAsync(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("productId is required", nameof(productId));
            using var response = await SendAsync(HttpMethod.Delete, $"/v1/products/{productId}");
            return response.IsSuccessStatusCode;
        }
    }
}
