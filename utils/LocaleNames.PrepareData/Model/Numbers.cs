using Newtonsoft.Json;
using System.Collections.Generic;

namespace LocaleNames.PrepareData.Model
{
    public partial class Numbers
    {
        [JsonProperty("currencies")]
        public Dictionary<string, Dictionary<string, string>> Currencies { get; set; }
    }
}
