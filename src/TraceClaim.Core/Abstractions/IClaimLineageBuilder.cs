using TraceClaim.Core.Claims;
using TraceClaim.Core.Relations;

namespace TraceClaim.Core.Abstractions;

/// <summary>
/// Builds semantic lineage relationships between claims.
///
/// A lineage builder does not decide which claim is true.
/// Its responsibility is only to identify temporal succession
/// between assertions that belong to the same logical claim history.
/// </summary>
public interface IClaimLineageBuilder
{
    IReadOnlyCollection<ClaimRelation> Build(
        IReadOnlyCollection<SoftwareClaim> claims);
}