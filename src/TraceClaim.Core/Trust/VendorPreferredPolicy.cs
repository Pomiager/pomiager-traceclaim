using TraceClaim.Core.Abstractions;
using TraceClaim.Core.Claims;

namespace TraceClaim.Core.Trust;

/// <summary>
/// Trust policy that gives precedence to claims issued by
/// the software vendor when a vendor claim is available.
/// </summary>
public sealed class VendorPreferredPolicy : ITrustPolicy
{
    private readonly string _vendorIssuer;

    public VendorPreferredPolicy(string vendorIssuer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vendorIssuer);

        _vendorIssuer = vendorIssuer;
    }

    public string Name => "vendor-preferred";

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

        var vendorClaim = claims
            .Where(c => string.Equals(
                c.Issuer,
                _vendorIssuer,
                StringComparison.OrdinalIgnoreCase))
            // The preferred vendor may issue multiple claims about the same software
            // over time. The most recent issuer-side statement must take precedence.
            //
            // IssuedAt is used here instead of ObservedAt because this policy is meant
            // to follow the vendor's own logical timeline, not the order in which
            // TraceClaim happened to ingest the claims.
            .OrderByDescending(c => c.IssuedAt)
            .FirstOrDefault();

        if (vendorClaim is not null)
        {
            return new TrustResolution
            {
                Status = PredicateToStatus(vendorClaim.Predicate),
                SelectedClaim = vendorClaim,
                Policy = Name,
                Reason =
                    $"A claim issued by the preferred vendor '{_vendorIssuer}' was selected."
            };
        }

        return new TrustResolution
        {
            Status = "unresolved",
            Policy = Name,
            Reason =
                $"No claim from the preferred vendor '{_vendorIssuer}' was found."
        };
    }

    private static string PredicateToStatus(string predicate)
    {
        if (string.Equals(
            predicate,
            "affected-by",
            StringComparison.OrdinalIgnoreCase))
        {
            return "affected";
        }

        if (string.Equals(
            predicate,
            "not-affected-by",
            StringComparison.OrdinalIgnoreCase))
        {
            return "not-affected";
        }

        return "unresolved";
    }
}