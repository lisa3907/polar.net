// This file provides the core HTTP plumbing for PolarClient, including
// HttpClient configuration and a redirect-aware SendAsync helper.
#if NETSTANDARD2_0 || NETSTANDARD2_1
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#else
#nullable enable
#endif

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PolarNet.Models;
using System.Net;

namespace PolarNet.Services
{
    /// <summary>
    /// Typed client for the Polar API.
    /// </summary>
    /// <remarks>
    /// This partial class encapsulates HTTP configuration (base URL, bearer token)
    /// and exposes endpoint-specific methods from other partials (Products, Customers, etc.).
    /// The internal <see cref="SendAsync"/> helper preserves request content across HTTP redirects
    /// and limits the number of redirects to avoid cycles.
    /// </remarks>
    public sealed partial class PolarClient : IDisposable
    {
        // Underlying HTTP client used for all requests
        private readonly HttpClient _http;
        // Default values sourced from options, used by helper methods where applicable
        private readonly string? _organizationId;
        private readonly string? _defaultProductId;
        private readonly string? _defaultPriceId;

        /// <summary>
        /// Initializes a new <see cref="PolarClient"/> with the provided options.
        /// </summary>
        /// <param name="options">Client options including access token and base URL.</param>
        public PolarClient(PolarClientOptions options)
            : this(options, (HttpMessageHandler?)null) { }

        /// <summary>
        /// Initializes a new <see cref="PolarClient"/> with a custom <see cref="HttpMessageHandler"/>.
        /// </summary>
        /// <param name="options">Client options including access token and base URL.</param>
        /// <param name="handler">Optional HTTP handler (e.g., for testing or proxying). When provided and it is an <see cref="HttpClientHandler"/>, auto-redirect is disabled.</param>
        public PolarClient(PolarClientOptions options, HttpMessageHandler? handler)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.AccessToken))
                throw new ArgumentException("AccessToken is required", nameof(options));
            var baseUrl = options.BaseUrl.TrimEnd('/');
            if (handler is null)
                handler = new HttpClientHandler { AllowAutoRedirect = false };
            else if (handler is HttpClientHandler h)
                h.AllowAutoRedirect = false;
            _http = new HttpClient(handler);
            _http.BaseAddress = new Uri(baseUrl);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _organizationId = options.OrganizationId;
            _defaultProductId = options.DefaultProductId;
            _defaultPriceId = options.DefaultPriceId;
        }

        /// <summary>
        /// Sends an HTTP request with redirect support and content preservation.
        /// </summary>
        /// <param name="method">HTTP method to use.</param>
        /// <param name="url">Relative or absolute target URL.</param>
        /// <param name="content">Optional request content; for redirects, JSON content is reconstructed to preserve the body.</param>
        /// <returns>The final <see cref="HttpResponseMessage"/> after following redirects.</returns>
        /// <remarks>
        /// - Follows up to five redirects (301, 302, 303, 307, 308).
        /// - For non-GET/HEAD methods with <see cref="StringContent"/>, the payload is re-materialized after each redirect.
        /// - Auto-redirect is disabled at the handler level to allow explicit control.
        /// </remarks>
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, HttpContent? content = null)
        {
            const int maxRedirects = 5;
            int redirects = 0;
            string currentUrl = url;
            while (true)
            {
                using var req = new HttpRequestMessage(method, currentUrl) { Content = content };
                var resp = await _http.SendAsync(req);
                if (resp.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther or HttpStatusCode.TemporaryRedirect || (int)resp.StatusCode == 308)
                {
                    if (redirects++ >= maxRedirects) return resp;
                    if (resp.Headers.Location is null) return resp;
                    currentUrl = resp.Headers.Location.IsAbsoluteUri ? resp.Headers.Location.ToString() : new Uri(_http.BaseAddress!, resp.Headers.Location).ToString();
                    if (content is StringContent sc && method != HttpMethod.Get && method != HttpMethod.Head)
                    {
                        var payload = await sc.ReadAsStringAsync();
                        content = new StringContent(payload, Encoding.UTF8, sc.Headers.ContentType?.MediaType ?? "application/json");
                    }
                    resp.Dispose();
                    continue;
                }
                return resp;
            }
        }

        /// <summary>
        /// Disposes the underlying <see cref="HttpClient"/> and frees resources.
        /// </summary>
        public void Dispose() => _http.Dispose();
    }
}
