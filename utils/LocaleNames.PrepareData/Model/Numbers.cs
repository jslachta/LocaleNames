using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace LocaleNames.PrepareData.Model
{
    public partial class Numbers
    {
        [JsonPropertyName("currencies")]
        public Dictionary<string, Dictionary<string, string>> Currencies { get; set; }
    }
}
