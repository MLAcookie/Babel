using System.Text.Json.Serialization;

namespace Babel.Models.OpenAi;

public class OpenAiChoice
{
    [JsonPropertyName("message")]
    public OpenAiMessage? Message { get; set; }
}