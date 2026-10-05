using TraceClaim.Core.Claims;
using TraceClaim.Core.Trust;

namespace TraceClaim.Core.Abstractions;

/// <summary>
/// Questo ci consentirà in futuro di avere:
/// ITrustPolicy
///    │
///    ├── ConservativeSecurityPolicy
///    ├── VendorPreferredPolicy
///    ├── AuthorityWeightedPolicy
///    └── CustomPolicy
/// </summary>
public interface ITrustPolicy
{
    string Name { get; }

    TrustResolution Resolve(
        IReadOnlyCollection<SoftwareClaim> claims);
}