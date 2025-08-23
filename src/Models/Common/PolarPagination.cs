using System.Text.Json.Serialization;

namespace PolarNet.Models
{
    /// <summary>
    /// Pagination metadata for list responses.
    /// </summary>
    /// <remarks>
    /// Use along with <see cref="PolarListResponse{T}"/> to paginate through resources.
    /// </remarks>
    public class PolarPagination
    {
        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Maximum available page index.
        /// </summary>
        [JsonPropertyName("max_page")]
        public int MaxPage { get; set; }
    }
}