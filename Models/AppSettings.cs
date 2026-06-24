using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Babel.Models;

public class AppSettings
{
    private static readonly string ConfigPath = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath)!,
        "appsettings.json");

    [JsonIgnore]
    public string ApiKey
    {
        get => ActiveProfile?.ApiKey ?? "";
        set { if (ActiveProfile != null) ActiveProfile.ApiKey = value; }
    }

    [JsonIgnore]
    public string Model
    {
        get => ActiveProfile?.Model ?? "";
        set { if (ActiveProfile != null) ActiveProfile.Model = value; }
    }

    [JsonIgnore]
    public string ApiEndpoint
    {
        get => ActiveProfile?.ApiEndpoint ?? "";
        set { if (ActiveProfile != null) ActiveProfile.ApiEndpoint = value; }
    }

    public string TargetLanguage { get; set; } = "English";

    public List<string> TabLanguages { get; set; } = [];

    public List<Profile> Profiles { get; set; } = [];
    public string ActiveProfileId { get; set; } = "";

    [JsonIgnore]
    public Profile? ActiveProfile => Profiles.FirstOrDefault(p => p.Id == ActiveProfileId);

    public static AppSettings Load()
    {
        var settings = LoadFromFile<AppSettings>();

        if (settings.ActiveProfile == null && settings.Profiles.Count > 0)
            settings.ActiveProfileId = settings.Profiles[0].Id;

        return settings;
    }

    public void Save()
    {
        SaveToFile(this);
    }

    private static T LoadFromFile<T>() where T : new()
    {
        if (!File.Exists(ConfigPath))
            return new T();

        try
        {
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Settings load failed: {ex.Message}");
            return new T();
        }
    }

    private static void SaveToFile<T>(T value)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(value, options));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Settings save failed: {ex.Message}");
        }
    }

    public AppSettings Clone()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }
}
