using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a paged list response in Polar APIs.
    /// </summary>
    /// <remarks>
    /// Contains the items for the current page and pagination metadata. Use together with <see cref="PolarPagination"/>.
    /// </remarks>
    public class PolarListResponse<T>
    {
        /// <summary>Items returned on the current page.</summary>
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new();

        /// <summary>Pagination metadata.</summary>
        [JsonPropertyName("pagination")]
        public PolarPagination Pagination { get; set; } = new();
    }
}
