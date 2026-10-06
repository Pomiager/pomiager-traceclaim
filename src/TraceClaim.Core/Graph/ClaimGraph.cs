using TraceClaim.Core.Claims;
using TraceClaim.Core.Relations;

namespace TraceClaim.Core.Graph;

/// <summary>
/// Represents an in-memory graph of software claims and the semantic
/// relationships between them.
///
/// The graph preserves the original claims as independent pieces of evidence.
/// It does not attempt to determine which claim is "true".
///
/// Resolution remains the responsibility of explicit consumer-controlled
/// trust policies.
/// </summary>
public sealed class ClaimGraph
{
    private readonly Dictionary<Guid, SoftwareClaim> _claims = [];
    private readonly List<ClaimRelation> _relations = [];

    /// <summary>
    /// Returns all claims currently stored in the graph.
    ///
    /// A read-only view is exposed so callers cannot mutate the internal
    /// collection directly and bypass graph integrity checks.
    /// </summary>
    public IReadOnlyCollection<SoftwareClaim> Claims =>
        _claims.Values.ToArray();

    /// <summary>
    /// Returns all semantic relationships currently stored in the graph.
    ///
    /// Relationships may represent support, conflict or temporal supersession.
    /// </summary>
    public IReadOnlyCollection<ClaimRelation> Relations =>
        _relations.AsReadOnly();

    /// <summary>
    /// Adds a claim to the graph.
    ///
    /// Claim identifiers must be unique. A duplicate identifier is rejected
    /// even when the remaining claim content differs, because the identifier
    /// is the primary identity of a claim within TraceClaim.
    /// </summary>
    public void AddClaim(SoftwareClaim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        if (_claims.ContainsKey(claim.Id))
        {
            throw new InvalidOperationException(
                $"A claim with id '{claim.Id}' already exists in the graph.");
        }

        _claims.Add(claim.Id, claim);
    }

    /// <summary>
    /// Adds a semantic relationship between two existing claims.
    ///
    /// Both endpoints must already exist in the graph. This prevents dangling
    /// edges and guarantees that every relation can be traversed safely.
    /// </summary>
    public void AddRelation(ClaimRelation relation)
    {
        ArgumentNullException.ThrowIfNull(relation);

        if (!_claims.ContainsKey(relation.SourceClaimId))
        {
            throw new InvalidOperationException(
                $"Source claim '{relation.SourceClaimId}' does not exist.");
        }

        if (!_claims.ContainsKey(relation.TargetClaimId))
        {
            throw new InvalidOperationException(
                $"Target claim '{relation.TargetClaimId}' does not exist.");
        }

        // Avoid storing the exact same semantic edge more than once.
        //
        // The human-readable Reason is intentionally not considered part of
        // relation identity. Two relations with the same endpoints and type
        // are semantically the same edge even if their explanations differ.
        var duplicateExists = _relations.Any(r =>
            r.SourceClaimId == relation.SourceClaimId &&
            r.TargetClaimId == relation.TargetClaimId &&
            r.Type == relation.Type);

        if (duplicateExists)
        {
            return;
        }

        _relations.Add(relation);
    }

    /// <summary>
    /// Retrieves a claim by its TraceClaim identifier.
    /// </summary>
    public SoftwareClaim? GetClaim(Guid claimId)
    {
        return _claims.GetValueOrDefault(claimId);
    }

    /// <summary>
    /// Returns all claims referring to the specified software subject.
    ///
    /// Subjects are compared case-insensitively because identifiers coming
    /// from independent metadata sources may differ only in casing.
    ///
    /// Normalisation rules for PURL will later be moved into a dedicated
    /// identity component rather than remaining a plain string comparison.
    /// </summary>
    public IReadOnlyCollection<SoftwareClaim> GetClaimsForSubject(
        string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        return _claims.Values
            .Where(c => string.Equals(
                c.Subject,
                subject,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.IssuedAt)
            .ToArray();
    }

    /// <summary>
    /// Returns all relations that involve the specified claim,
    /// regardless of whether the claim is the source or target.
    /// </summary>
    public IReadOnlyCollection<ClaimRelation> GetRelationsForClaim(
        Guid claimId)
    {
        return _relations
            .Where(r =>
                r.SourceClaimId == claimId ||
                r.TargetClaimId == claimId)
            .ToArray();
    }

    /// <summary>
    /// Returns all explicitly declared conflict relationships.
    ///
    /// Conflict detection itself is performed by IClaimConflictDetector.
    /// The graph only stores the resulting relationship.
    /// </summary>
    public IReadOnlyCollection<ClaimRelation> GetConflicts()
    {
        return _relations
            .Where(r => r.Type == ClaimRelationType.ConflictsWith)
            .ToArray();
    }

    /// <summary>
    /// Returns claims that have not been superseded by a newer claim
    /// in the graph.
    ///
    /// Given the relation:
    ///
    ///     NewClaim --Supersedes--> OldClaim
    ///
    /// the target (OldClaim) is no longer considered current.
    ///
    /// A claim can still participate in conflicts while also being current.
    /// "Current" only refers to temporal supersession, not semantic truth.
    /// </summary>
    public IReadOnlyCollection<SoftwareClaim> GetCurrentClaims()
    {
        var supersededClaimIds = _relations
            .Where(r => r.Type == ClaimRelationType.Supersedes)
            .Select(r => r.TargetClaimId)
            .ToHashSet();

        return _claims.Values
            .Where(c => !supersededClaimIds.Contains(c.Id))
            .OrderBy(c => c.IssuedAt)
            .ToArray();
    }
}