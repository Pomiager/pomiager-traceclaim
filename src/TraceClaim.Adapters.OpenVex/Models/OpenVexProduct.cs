using System.Text.Json.Serialization;

namespace TraceClaim.Adapters.OpenVex.Models;

/// <summary>
/// Represents a software product referenced by an OpenVEX statement.
/// </summary>
public sealed class OpenVexProduct
{
    /// <summary>
    /// Product identifier.
    ///
    /// OpenVEX commonly uses PURL values directly as @id, which maps
    /// naturally to TraceClaim's Subject field.
    /// </summary>
    [JsonPropertyName("@id")]
    public required string Id { get; init; }
}