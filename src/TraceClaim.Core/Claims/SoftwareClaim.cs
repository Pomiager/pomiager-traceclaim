namespace TraceClaim.Core.Claims;

/// <summary>
/// Represents a verifiable assertion made by an issuer
/// about a software subject.
/// </summary>
public sealed record SoftwareClaim
{
    /// <summary>
    /// Unique identifier of the claim.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Software subject of the claim.
    /// Package URL (PURL) SHOULD be used whenever applicable.
    /// Example: pkg:nuget/acme.security@2.1.0
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Semantic relationship asserted by the issuer.
    /// Examples: affected-by, not-affected-by, licensed-under.
    /// </summary>
    public required string Predicate { get; init; }

    /// <summary>
    /// Object of the assertion.
    /// Example: CVE-2026-1234.
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// Authority or source that issued the claim.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Time at which the claim was observed or issued.
    /// </summary>
    public required DateTimeOffset ObservedAt { get; init; }

    /// <summary>
    /// Optional reference to the original evidence
    /// from which the claim was derived.
    /// </summary>
    public Uri? EvidenceUri { get; init; }

    /// <summary>
    /// Cryptographic digest of the canonical representation
    /// of the claim.
    /// </summary>
    public string? Hash { get; init; }
}