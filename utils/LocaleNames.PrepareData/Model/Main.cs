using System.Text.Json.Serialization;

namespace LocaleNames.PrepareData.Model
{
    public partial class Main
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
}
