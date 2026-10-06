using System.Text.Json.Serialization;

namespace TraceClaim.Adapters.OpenVex.Models;

/// <summary>
/// Represents the subset of the OpenVEX v0.2.0 document model
/// currently consumed by TraceClaim.
///
/// TraceClaim deliberately models only the fields required to
/// normalize OpenVEX statements into SoftwareClaim instances.
///
/// The model can be extended later without coupling TraceClaim.Core
/// to the full OpenVEX specification.
/// </summary>
public sealed class OpenVexDocument
{
    /// <summary>
    /// JSON-LD context identifying the OpenVEX namespace.
    ///
    /// Example:
    /// https://openvex.dev/ns/v0.2.0
    /// </summary>
    [JsonPropertyName("@context")]
    public string? Context { get; init; }

    /// <summary>
    /// Optional globally unique identifier of the VEX document.
    /// </summary>
    [JsonPropertyName("@id")]
    public string? Id { get; init; }

    /// <summary>
    /// Human-readable identity of the document author.
    ///
    /// OpenVEX uses the author as the party responsible for the VEX
    /// statements contained in the document.
    /// </summary>
    [JsonPropertyName("author")]
    public required string Author { get; init; }

    /// <summary>
    /// Timestamp at which the VEX document was issued.
    ///
    /// Statements may define their own timestamp. When they do not,
    /// OpenVEX inheritance rules allow the document timestamp to be used.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Monotonically increasing VEX document version.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; init; }

    /// <summary>
    /// Vulnerability impact statements contained in the document.
    /// </summary>
    [JsonPropertyName("statements")]
    public IReadOnlyCollection<OpenVexStatement> Statements { get; init; } = [];
}