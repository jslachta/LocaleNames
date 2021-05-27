using Newtonsoft.Json;

namespace PrepareLocaleData.Model
{
    public partial class Main
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
