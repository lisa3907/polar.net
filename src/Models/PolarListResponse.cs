using Newtonsoft.Json;
using System.Collections.Generic;

namespace PolarNet.Models
{
    /// <summary>
    /// Represents a paged list response in Polar APIs.
    /// </summary>
    public class PolarListResponse<T>
    {
        /// <summary>Items returned on the current page.</summary>
        [JsonProperty("items")]
        public List<T> Items { get; set; } = new();

        /// <summary>Pagination metadata.</summary>
        [JsonProperty("pagination")]
        public PolarPagination Pagination { get; set; } = new();
    }
}
