using Newtonsoft.Json;

namespace PrepareLocaleData.Model
{
    public partial class Version
    {
        [JsonProperty("_cldrVersion")]
        public string CldrVersion { get; set; }
    }
}
