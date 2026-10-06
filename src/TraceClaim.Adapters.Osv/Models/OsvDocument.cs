using System.Text.Json.Serialization;

namespace TraceClaim.Adapters.Osv.Models;

/// <summary>
/// Represents the subset of the OpenSSF OSV schema currently consumed
/// by TraceClaim.
///
/// This is intentionally NOT a complete implementation of the OSV schema.
/// The adapter should model only the fields it needs and remain tolerant
/// of additional fields present in upstream OSV documents.
///
/// Future TraceClaim versions may extend this model to include ranges,
/// severity, aliases, references and withdrawal information.
/// </summary>
public sealed class OsvDocument
{
    /// <summary>
    /// Identifier assigned to the vulnerability entry by its source database.
    ///
    /// Examples:
    /// GHSA-xxxx-xxxx-xxxx
    /// CVE-2026-1234
    /// OSV-2026-123
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Time at which the OSV entry is considered to have been published.
    ///
    /// This value maps naturally to SoftwareClaim.IssuedAt because it
    /// belongs to the source advisory rather than to TraceClaim's ingestion.
    /// </summary>
    [JsonPropertyName("published")]
    public DateTimeOffset? Published { get; init; }

    /// <summary>
    /// Time at which the OSV entry was most recently modified.
    ///
    /// Modified is deliberately kept separate from Published. A future
    /// adapter version may use this field to create new claim revisions
    /// or reconstruct advisory-level lineage.
    /// </summary>
    [JsonPropertyName("modified")]
    public DateTimeOffset? Modified { get; init; }

    /// <summary>
    /// Packages and package versions reported as affected by this advisory.
    /// </summary>
    [JsonPropertyName("affected")]
    public IReadOnlyCollection<OsvAffected> Affected { get; init; } = [];
}