using System.Text.Json.Serialization;

public record class Req(
    [property: JsonPropertyName("k")] string K,
    [property: JsonPropertyName("v"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? V);
