using System.Text.Json.Serialization;

namespace LocaleNames.PrepareData.Model
{
    public partial class Version
    {
        [JsonPropertyName("_cldrVersion")]
        public string CldrVersion { get; set; }
    }
}
