using System.Text.Json.Serialization;

namespace LocaleNames.PrepareData.Model
{
    public partial class Data
    {
        [JsonPropertyName("identity")]
        public Identity Identity { get; set; }

        [JsonPropertyName("localeDisplayNames")]
        public LocaleDisplayNames LocaleDisplayNames { get; set; }

        [JsonPropertyName("numbers")]
        public Numbers Numbers { get; set; }
    }
}
