using TraceClaim.Core.Abstractions;
using TraceClaim.Core.Claims;

namespace TraceClaim.Core.Trust;

public sealed class ConservativeSecurityPolicy : ITrustPolicy
{
    public string Name => "conservative-security";

    public TrustResolution Resolve(
        IReadOnlyCollection<SoftwareClaim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        if (claims.Count == 0)
        {
            return new TrustResolution
            {
                Status = "unresolved",
                Policy = Name,
                Reason = "No claims were provided."
            };
        }

        var affectedClaim = claims.FirstOrDefault(
            c => string.Equals(
                c.Predicate,
                "affected-by",
                StringComparison.OrdinalIgnoreCase));

        if (affectedClaim is not null)
        {
            return new TrustResolution
            {
                Status = "affected",
                SelectedClaim = affectedClaim,
                Policy = Name,
                Reason =
                    "At least one source reports the software as affected."
            };
        }

        var notAffectedClaim = claims.FirstOrDefault(
            c => string.Equals(
                c.Predicate,
                "not-affected-by",
                StringComparison.OrdinalIgnoreCase));

        if (notAffectedClaim is not null)
        {
            return new TrustResolution
            {
                Status = "not-affected",
                SelectedClaim = notAffectedClaim,
                Policy = Name,
                Reason =
                    "No source reports the software as affected."
            };
        }

        return new TrustResolution
        {
            Status = "unresolved",
            Policy = Name,
            Reason = "The supplied claims cannot be resolved by this policy."
        };
    }
}