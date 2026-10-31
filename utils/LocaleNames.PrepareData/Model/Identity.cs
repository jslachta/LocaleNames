using System.Text.Json.Serialization;

namespace LocaleNames.PrepareData.Model
{
    public partial class Identity
    {
        [JsonPropertyName("version")]
        public Version Version { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }
}
