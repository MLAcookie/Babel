using System.Text.Json.Serialization;

namespace Babel.Models.OpenAi;

public class OpenAiModelItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
}
