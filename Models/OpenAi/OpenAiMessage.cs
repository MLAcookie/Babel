using System.Text.Json.Serialization;

namespace Babel.Models.OpenAi;

public class OpenAiMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}