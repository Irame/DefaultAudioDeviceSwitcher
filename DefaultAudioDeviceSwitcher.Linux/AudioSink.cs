using System.Collections.Generic;
using System.Text.Json.Serialization;

public class AudioSink
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("driver")]
    public string Driver { get; set; }

    [JsonPropertyName("sample_specification")]
    public string SampleSpecification { get; set; }

    [JsonPropertyName("channel_map")]
    public string ChannelMap { get; set; }

    [JsonPropertyName("owner_module")]
    public long OwnerModule { get; set; }

    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    [JsonPropertyName("volume")]
    public Dictionary<string, ChannelVolume> Volume { get; set; }

    [JsonPropertyName("balance")]
    public double Balance { get; set; }

    [JsonPropertyName("base_volume")]
    public ChannelVolume BaseVolume { get; set; }

    [JsonPropertyName("monitor_source")]
    public string MonitorSource { get; set; }

    [JsonPropertyName("latency")]
    public Latency Latency { get; set; }

    [JsonPropertyName("flags")]
    public List<string> Flags { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, string> Properties { get; set; }

    [JsonPropertyName("ports")]
    public List<Port> Ports { get; set; }

    [JsonPropertyName("active_port")]
    public string ActivePort { get; set; }

    [JsonPropertyName("formats")]
    public List<string> Formats { get; set; }
}

public class ChannelVolume
{
    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("value_percent")]
    public string ValuePercent { get; set; }

    [JsonPropertyName("db")]
    public string Db { get; set; }
}

public class Latency
{
    [JsonPropertyName("actual")]
    public double Actual { get; set; }

    [JsonPropertyName("configured")]
    public double Configured { get; set; }
}

public class Port
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("availability_group")]
    public string AvailabilityGroup { get; set; }

    [JsonPropertyName("availability")]
    public string Availability { get; set; }
}