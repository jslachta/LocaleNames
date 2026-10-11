using Newtonsoft.Json;

namespace LocaleNames.PrepareData.Model
{
    public partial class Main
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
