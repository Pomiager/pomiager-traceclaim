namespace TraceClaim.Core.Claims;

/// <summary>
/// Represents a verifiable assertion made by an issuer
/// about a software subject.
/// </summary>
public sealed record SoftwareClaim
{
    /// <summary>
    /// Unique identifier of the claim inside TraceClaim.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Software subject of the claim.
    ///
    /// Package URL (PURL) SHOULD be used whenever applicable
    /// so that claims originating from different ecosystems
    /// can refer to the same software component consistently.
    ///
    /// Example:
    /// pkg:nuget/acme.security@2.1.0
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Semantic relationship asserted by the issuer.
    ///
    /// Examples:
    /// affected-by
    /// not-affected-by
    /// licensed-under
    /// fixed-by
    /// </summary>
    public required string Predicate { get; init; }

    /// <summary>
    /// Object of the assertion.
    ///
    /// Examples:
    /// CVE-2026-1234
    /// MIT
    /// pkg:nuget/acme.security@2.1.1
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// Authority or source that issued the claim.
    ///
    /// The issuer identifies who is responsible for the assertion.
    /// It is intentionally kept separate from cryptographic verification:
    /// an issuer identity may be known even before a signature is verified.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Time at which the original issuer states that the claim was issued.
    ///
    /// This timestamp belongs to the source domain and describes when the
    /// assertion became valid or was published according to the issuer.
    ///
    /// It must not be confused with ObservedAt, which represents the time
    /// at which TraceClaim discovered or ingested the claim.
    ///
    /// Example:
    /// A vendor advisory may have been published on September 1st but only
    /// discovered by TraceClaim on September 4th.
    /// </summary>
    public required DateTimeOffset IssuedAt { get; init; }

    /// <summary>
    /// Time at which TraceClaim observed, imported or received the claim.
    ///
    /// This timestamp belongs to TraceClaim's own ingestion history and
    /// can therefore differ from IssuedAt.
    ///
    /// Keeping both timestamps allows the system to reconstruct:
    /// 1. the issuer's logical claim history;
    /// 2. TraceClaim's own observation history.
    /// </summary>
    public required DateTimeOffset ObservedAt { get; init; }

    /// <summary>
    /// Optional reference to the original evidence from which
    /// the claim was derived.
    ///
    /// The evidence itself is not necessarily stored by TraceClaim.
    /// The URI merely preserves a link to the authoritative source
    /// whenever such a source exists.
    /// </summary>
    public Uri? EvidenceUri { get; init; }

    /// <summary>
    /// Cryptographic digest of the canonical representation of the claim.
    ///
    /// The hash is optional at this stage because canonicalisation
    /// has not yet been introduced into the domain model.
    ///
    /// A future component will be responsible for producing a stable,
    /// deterministic representation before calculating the digest.
    /// </summary>
    public string? Hash { get; init; }
}