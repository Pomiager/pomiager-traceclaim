using System.Text.Json.Serialization;

namespace TraceClaim.Adapters.OpenVex.Models;

/// <summary>
/// Represents a single OpenVEX assertion describing the impact
/// of one vulnerability on one or more products.
/// </summary>
public sealed class OpenVexStatement
{
    /// <summary>
    /// Optional identifier that can make the statement externally referenceable.
    /// </summary>
    [JsonPropertyName("@id")]
    public string? Id { get; init; }

    /// <summary>
    /// Vulnerability referenced by the statement.
    /// </summary>
    [JsonPropertyName("vulnerability")]
    public required OpenVexVulnerability Vulnerability { get; init; }

    /// <summary>
    /// Product identifiers to which the statement applies.
    ///
    /// OpenVEX allows product information to be inherited from an outer
    /// document in some embedding scenarios. TraceClaim v0.1 initially
    /// expects products to be present directly in the statement.
    /// </summary>
    [JsonPropertyName("products")]
    public IReadOnlyCollection<OpenVexProduct> Products { get; init; } = [];

    /// <summary>
    /// OpenVEX impact status.
    ///
    /// Valid values defined by the specification include:
    /// not_affected, affected, fixed, under_investigation.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    /// Optional statement-level timestamp.
    ///
    /// If omitted, OpenVEX inheritance rules allow the document timestamp
    /// to represent when the statement was known to be true.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>
    /// Machine-readable explanation of why a product is not affected.
    ///
    /// OpenVEX requires either this field or impact_statement when
    /// status is not_affected.
    /// </summary>
    [JsonPropertyName("justification")]
    public string? Justification { get; init; }

    /// <summary>
    /// Optional free-form explanation of why the vulnerability
    /// does not impact the product.
    /// </summary>
    [JsonPropertyName("impact_statement")]
    public string? ImpactStatement { get; init; }

    /// <summary>
    /// Optional remediation or mitigation guidance for affected products.
    /// </summary>
    [JsonPropertyName("action_statement")]
    public string? ActionStatement { get; init; }
}