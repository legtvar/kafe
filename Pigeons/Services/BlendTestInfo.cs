using System.Text.Json.Serialization;

namespace Kafe.Pigeons.Services;

public record PigeonsTestResultJson
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("datablock")]
    public string? Datablock { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("traceback")]
    public string? Traceback { get; set; }
};
