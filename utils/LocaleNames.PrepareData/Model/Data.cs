using Newtonsoft.Json;

namespace LocaleNames.PrepareData.Model
{
    public partial class Data
    {
        [JsonProperty("identity")]
        public Identity Identity { get; set; }

        [JsonProperty("localeDisplayNames")]
        public LocaleDisplayNames LocaleDisplayNames { get; set; }

        [JsonProperty("numbers")]
        public Numbers Numbers { get; set; }
    }
}
