// Enable nullable reference types for this file to allow string? annotations
#nullable enable

namespace PolarNet;

/// <summary>
/// Options for configuring the Polar API client.
/// </summary>
public sealed class PolarClientOptions
{
    /// <summary>
    /// Polar access token (Organization Access Token). Required.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>
    /// Base URL without trailing /v1. Defaults to sandbox.
    /// </summary>
    public string BaseUrl { get; set; } = "https://sandbox-api.polar.sh"; // Without /v1
    /// <summary>
    /// Default organization ID used for listing/filtering where applicable.
    /// </summary>
    public string? OrganizationId { get; set; }
    /// <summary>
    /// Optional default product ID used by helper methods.
    /// </summary>
    public string? DefaultProductId { get; set; }
    /// <summary>
    /// Optional default price ID used by helper methods like checkout/subscription creation.
    /// </summary>
    public string? DefaultPriceId { get; set; }
}
