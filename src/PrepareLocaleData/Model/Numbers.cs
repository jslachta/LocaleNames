using Newtonsoft.Json;
using System.Collections.Generic;

namespace PrepareLocaleData.Model
{
    public partial class Numbers
    {
        [JsonProperty("currencies")]
        public Dictionary<string, Dictionary<string, string>> Currencies { get; set; }
    }
}
