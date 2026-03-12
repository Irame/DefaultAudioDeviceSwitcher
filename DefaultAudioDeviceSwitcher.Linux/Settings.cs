using System.Text.Json;

namespace DefaultAudioDeviceSwitcher.Linux;

internal class Settings
{
    private string _filePath = null!;

    public string? HeadsetName { get; set; }
    public string? HeadsetDescription { get; set; }
        
    public string? SpeakerName { get; set; }
    public string? SpeakerDescription { get; set; }

    public static Settings Load(string filePath)
    {
        Settings settings;
        if (File.Exists(filePath))
            settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(filePath)) ?? new Settings();
        else
            settings = new Settings();
        settings._filePath = filePath;

        return settings;
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(this);
        File.WriteAllText(_filePath, json);
    }
}