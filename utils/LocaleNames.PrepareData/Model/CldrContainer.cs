using Newtonsoft.Json;

namespace LocaleNames.PrepareData.Model
{
    public partial class CldrContainer
    {
        [JsonProperty("main")]
        public Main Main { get; set; }
    }
}
