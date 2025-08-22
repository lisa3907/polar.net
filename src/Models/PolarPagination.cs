using Newtonsoft.Json;

namespace PolarNet.Models
{
    /// <summary>
    /// Pagination metadata for list responses.
    /// </summary>
    public class PolarPagination
    {
        /// <summary>Total number of items across all pages.</summary>
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        /// <summary>Maximum available page index.</summary>
        [JsonProperty("max_page")]
        public int MaxPage { get; set; }
    }
}
