using System.Text.Json.Serialization;

namespace Babel.Models.OpenAi;

public class OpenAiChatResponse
{
    [JsonPropertyName("choices")]
    public List<OpenAiChoice>? Choices { get; set; }
}