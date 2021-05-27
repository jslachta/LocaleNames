using Newtonsoft.Json;

namespace PrepareLocaleData.Model
{
    public partial class CldrContainer
    {
        [JsonProperty("main")]
        public Main Main { get; set; }
    }
}
