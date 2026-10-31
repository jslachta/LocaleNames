using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LocaleNames.PrepareData.Model
{
    public partial class LocaleDisplayNames
    {
        [JsonPropertyName("languages")]
        public Dictionary<string, string> Languages { get; set; }

        [JsonPropertyName("territories")]
        public Dictionary<string, string> Territories { get; set; }
    }
}
