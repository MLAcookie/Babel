using Babel.Models;

namespace Babel.Services;

public interface IAiModelService
{
    Task<string> TranslateAsync(string text, AppSettings settings, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListModelsAsync(string apiEndpoint, string apiKey, CancellationToken ct = default);
}
