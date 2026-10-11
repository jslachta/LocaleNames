using Newtonsoft.Json;

namespace LocaleNames.PrepareData.Model
{
    public partial class Version
    {
        [JsonProperty("_cldrVersion")]
        public string CldrVersion { get; set; }
    }
}
