namespace TraceClaim.Core.Relations;

/// <summary>
/// Represents a semantic relationship between two claims.
/// </summary>
public sealed record ClaimRelation
{
    public required Guid SourceClaimId { get; init; }

    public required Guid TargetClaimId { get; init; }

    public required ClaimRelationType Type { get; init; }

    /// <summary>
    /// Optional human-readable explanation of why
    /// the relationship was established.
    /// </summary>
    public string? Reason { get; init; }
}