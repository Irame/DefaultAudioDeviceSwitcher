using System.Text.Json.Serialization;

namespace DefaultAudioDeviceSwitcher.Linux;

public class ShortSinkInput
{
    [JsonPropertyName("index")]
    public ulong Index { get; set; }

    [JsonPropertyName("sink")]
    public ulong Sink { get; set; }

    [JsonPropertyName("client")]
    public string Client { get; set; }

    [JsonPropertyName("driver")]
    public string Driver { get; set; }

    [JsonPropertyName("sample_specification")]
    public string SampleSpecification { get; set; }
}