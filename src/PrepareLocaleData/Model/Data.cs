using Newtonsoft.Json;

namespace PrepareLocaleData.Model
{
    public partial class Data
    {
        [JsonProperty("identity")]
        public Identity Identity { get; set; }

        [JsonProperty("localeDisplayNames")]
        public LocaleDisplayNames LocaleDisplayNames { get; set; }
    }
}
