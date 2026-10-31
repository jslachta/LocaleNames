using System.Text.Json.Serialization;

namespace LocaleNames.PrepareData.Model
{
    public partial class CldrContainer
    {
        [JsonPropertyName("main")]
        public Main Main { get; set; }
    }
}
