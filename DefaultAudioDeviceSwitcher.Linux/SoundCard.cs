namespace DefaultAudioDeviceSwitcher.Linux;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class SoundCard
{
    [JsonPropertyName("index")]
    public required int Index { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; } = string.Empty;

    [JsonPropertyName("driver")]
    public required string Driver { get; set; } = string.Empty;

    [JsonPropertyName("owner_module")]
    public ulong? OwnerModule { get; set; } = null;

    [JsonPropertyName("properties")]
    public required Dictionary<string, string> Properties { get; set; } = new();

    [JsonPropertyName("profiles")]
    public required Dictionary<string, Profile> Profiles { get; set; } = new();

    [JsonPropertyName("active_profile")]
    public required string ActiveProfile { get; set; } = string.Empty;

    [JsonPropertyName("ports")]
    public required Dictionary<string, Port> Ports { get; set; } = new();
}

public class Profile
{
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("sinks")]
    public required int Sinks { get; set; }

    [JsonPropertyName("sources")]
    public required int Sources { get; set; }

    [JsonPropertyName("priority")]
    public required int Priority { get; set; }

    [JsonPropertyName("available")]
    public required bool Available { get; set; }
}

public class Port
{
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("priority")]
    public required int Priority { get; set; }

    [JsonPropertyName("latency_offset")]
    public required string LatencyOffset { get; set; }

    [JsonPropertyName("availability_group")]
    public required string AvailabilityGroup { get; set; }

    [JsonPropertyName("availability")]
    public required string Availability { get; set; }

    [JsonPropertyName("properties")]
    public required Dictionary<string, string> Properties { get; set; } = new();

    [JsonPropertyName("profiles")]
    public required List<string> Profiles { get; set; } = [];
}

