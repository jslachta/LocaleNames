using Newtonsoft.Json;

namespace LocaleNames.PrepareData.Model
{
    public partial class Identity
    {
        [JsonProperty("version")]
        public Version Version { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
