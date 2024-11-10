using Newtonsoft.Json;

namespace jewelryRazorPage
{
    public class ODataResponse<T>
    {
        public List<T> Value { get; set; }
        [JsonProperty("@odata.count")]
        public int Count { get; set; }
    }
}
