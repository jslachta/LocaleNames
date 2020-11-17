using System.Collections.Generic;
using Newtonsoft.Json;

namespace PrepareLocaleData.Model
{
    public partial class CldrContainer
    {
        [JsonProperty("main")]
        public Main Main { get; set; }
    }

    public partial class Main
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("identity")]
        public Identity Identity { get; set; }

        [JsonProperty("localeDisplayNames")]
        public LocaleDisplayNames LocaleDisplayNames { get; set; }
    }

    public partial class Identity
    {
        [JsonProperty("version")]
        public Version Version { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("_cldrVersion")]
        public string CldrVersion { get; set; }
    }

    public partial class LocaleDisplayNames
    {
        [JsonProperty("languages")]
        public Dictionary<string, string> Languages { get; set; }

        [JsonProperty("territories")]
        public Dictionary<string, string> Territories { get; set; }
    }
}
