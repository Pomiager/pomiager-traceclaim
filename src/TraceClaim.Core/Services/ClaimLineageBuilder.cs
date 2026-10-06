using TraceClaim.Core.Abstractions;
using TraceClaim.Core.Claims;
using TraceClaim.Core.Relations;

namespace TraceClaim.Core.Services;

/// <summary>
/// Builds temporal supersession relationships between claims issued
/// by the same authority about the same software subject and object.
///
/// The builder intentionally does not alter or remove any claim.
/// Earlier claims remain part of the historical record even when
/// they are superseded by later assertions.
/// </summary>
public sealed class ClaimLineageBuilder : IClaimLineageBuilder
{
    public IReadOnlyCollection<ClaimRelation> Build(
        IReadOnlyCollection<SoftwareClaim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        if (claims.Count < 2)
        {
            return [];
        }

        var relations = new List<ClaimRelation>();

        // Claims are grouped by their logical identity.
        //
        // For this first implementation, two claims belong to the same
        // lineage when they refer to:
        // - the same software subject;
        // - the same object;
        // - the same issuer.
        //
        // The predicate is deliberately NOT part of the grouping key.
        // This is important because the same issuer may change its assertion
        // over time, for example:
        //
        // affected-by -> not-affected-by
        //
        // If predicate were part of the key, those two claims would appear
        // as unrelated statements and no supersession relationship could
        // be reconstructed.
        var groups = claims
            .GroupBy(
                c => new
                {
                    Subject = c.Subject.ToUpperInvariant(),
                    Object = c.Object.ToUpperInvariant(),
                    Issuer = c.Issuer.ToUpperInvariant()
                });

        foreach (var group in groups)
        {
            // IssuedAt represents the issuer-side timeline and is therefore
            // the correct ordering field for semantic lineage.
            //
            // ObservedAt is not used here because ingestion order may differ
            // from the order in which the issuer actually published claims.
            var orderedClaims = group
                .OrderBy(c => c.IssuedAt)
                .ThenBy(c => c.ObservedAt)
                .ToList();

            for (var index = 0; index < orderedClaims.Count - 1; index++)
            {
                var previous = orderedClaims[index];
                var next = orderedClaims[index + 1];

                // Every later claim from the same issuer supersedes the
                // immediately preceding claim in that issuer's timeline.
                //
                // This does NOT mean the previous claim becomes invalid
                // historical data. It only means that the issuer has since
                // published a newer assertion about the same subject/object.
                relations.Add(new ClaimRelation
                {
                    SourceClaimId = next.Id,
                    TargetClaimId = previous.Id,
                    Type = ClaimRelationType.Supersedes,
                    Reason =
                        "A newer claim from the same issuer supersedes the previous claim."
                });
            }
        }

        return relations;
    }
}