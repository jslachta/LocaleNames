using System.Collections.Generic;
using Newtonsoft.Json;

namespace PrepareLocaleData.Model
{
    public partial class LocaleDisplayNames
    {
        [JsonProperty("languages")]
        public Dictionary<string, string> Languages { get; set; }

        [JsonProperty("territories")]
        public Dictionary<string, string> Territories { get; set; }
    }
}
