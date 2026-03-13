using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DefaultAudioDeviceSwitcher.Linux;

internal class NameDescSetting
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}

internal class SinkSettings
{
    public NameDescSetting? Card { get; set; }
    public NameDescSetting? Profile { get; set; }
}

internal class Settings
{
    private string _filePath = null!;

    public SinkSettings Headset { get; } = new();

    public SinkSettings Speaker { get; } = new();

    private static JsonSerializerOptions _jsonOptions = new() {
        PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
#pragma warning disable SYSLIB0020
        IgnoreNullValues = true,
#pragma warning restore SYSLIB0020
    };

    public static Settings Load(string filePath)
    {
        Settings settings;
        if (File.Exists(filePath))
        {
            try
            {
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(filePath), _jsonOptions) ?? new Settings();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error loading settings file: {e.Message}");
                settings  = new Settings();
            }
        }
        else
        {
            settings = new Settings();
        }

        settings._filePath = filePath;

        return settings;
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this, _jsonOptions);
        File.WriteAllText(_filePath, json);
        OnSave?.Invoke();
    }

    public event Action? OnSave;
}