using System.Net.Http;
using System.Net.Http.Json;
using Babel.Models;
using Babel.Models.OpenAi;

namespace Babel.Services;

public class OpenAiService : IAiModelService
{
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    public async Task<string> TranslateAsync(string text, AppSettings settings, CancellationToken ct = default)
    {
        var requestBody = new
        {
            model = settings.Model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content =
                        $"You are a professional translator. Translate the following text to {settings.TargetLanguage}. Only respond with the translated text, no explanations, no quotations, no additional remarks."
                },
                new { role = "user", content = text }
            }
        };

        using var request =
            new HttpRequestMessage(HttpMethod.Post, $"{settings.ApiEndpoint.TrimEnd('/')}/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
        request.Content = JsonContent.Create(requestBody);

        using var response = await Client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>(cancellationToken: ct);
        return result?.Choices?[0]?.Message?.Content?.Trim() ?? "";
    }

    public async Task<IReadOnlyList<string>> ListModelsAsync(string apiEndpoint, string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiEndpoint))
            throw new ArgumentException("API Endpoint is required.", nameof(apiEndpoint));

        var baseUrl = apiEndpoint.TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/models");
        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        using var response = await Client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAiModelsResponse>(cancellationToken: ct);
        if (result?.Data == null)
            return Array.Empty<string>();

        return result.Data
            .Where(m => !string.IsNullOrWhiteSpace(m.Id))
            .Select(m => m.Id)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
