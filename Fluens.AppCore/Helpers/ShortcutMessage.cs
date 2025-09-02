using System.Text.Json.Serialization;

namespace Fluens.AppCore.Helpers;

public record ShortcutMessage
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("ctrl")]
    public bool? Ctrl { get; set; }

    [JsonPropertyName("shift")]
    public bool? Shift { get; set; }
}