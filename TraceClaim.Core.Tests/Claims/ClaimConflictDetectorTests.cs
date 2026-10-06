using TraceClaim.Core.Claims;
using TraceClaim.Core.Services;

namespace TraceClaim.Core.Tests.Claims;

public class ClaimConflictDetectorTests
{
    [Fact]
    public void SamePackageAndCve_WithOppositePredicates_IsConflict()
    {
        var osvClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://osv.dev",
            IssuedAt = DateTimeOffset.UtcNow,
            ObservedAt = DateTimeOffset.UtcNow
        };

        var vendorClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",
            IssuedAt = DateTimeOffset.UtcNow,
            ObservedAt = DateTimeOffset.UtcNow
        };

        var detector = new ClaimConflictDetector();

        var result = detector.AreConflicting(osvClaim, vendorClaim);

        Assert.True(result);
    }

    [Fact]
    public void DifferentPackage_IsNotConflict()
    {
        var first = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://osv.dev",
            IssuedAt = DateTimeOffset.UtcNow,
            ObservedAt = DateTimeOffset.UtcNow
        };

        var second = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/another.package@1.0.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",
            IssuedAt = DateTimeOffset.UtcNow,
            ObservedAt = DateTimeOffset.UtcNow
        };

        var detector = new ClaimConflictDetector();

        var result = detector.AreConflicting(first, second);

        Assert.False(result);
    }
}