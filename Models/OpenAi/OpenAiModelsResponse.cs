using System.Text.Json.Serialization;

namespace Babel.Models.OpenAi;

public class OpenAiModelsResponse
{
    [JsonPropertyName("data")]
    public List<OpenAiModelItem>? Data { get; set; }
}
