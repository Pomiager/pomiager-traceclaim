using TraceClaim.Core.Claims;

namespace TraceClaim.Core.Trust;

public sealed record TrustResolution
{
    public required string Status { get; init; }

    public SoftwareClaim? SelectedClaim { get; init; }

    public required string Policy { get; init; }

    public required string Reason { get; init; }
}