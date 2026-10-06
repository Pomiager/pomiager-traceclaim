using TraceClaim.Core.Claims;
using TraceClaim.Core.Trust;

namespace TraceClaim.Core.Tests.Trust;

public class ConservativeSecurityPolicyTests
{
    [Fact]
    public void ConflictingClaims_SelectsAffectedClaim()
    {
        var osvClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://osv.dev",
            IssuedAt = new DateTimeOffset(
    2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            ObservedAt = new DateTimeOffset(
    2026, 9, 2, 8, 0, 0, TimeSpan.Zero)
        };

        var vendorClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",
            IssuedAt = new DateTimeOffset(
    2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            ObservedAt = new DateTimeOffset(
    2026, 9, 2, 8, 0, 0, TimeSpan.Zero)
        };

        var policy = new ConservativeSecurityPolicy();

        var resolution = policy.Resolve(
            [osvClaim, vendorClaim]);

        Assert.Equal("affected", resolution.Status);
        Assert.Equal(osvClaim.Id, resolution.SelectedClaim?.Id);
        Assert.Equal(
            "conservative-security",
            resolution.Policy);
    }

    [Fact]
    public void OnlyNotAffectedClaims_ReturnsNotAffected()
    {
        var vendorClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",
            IssuedAt = new DateTimeOffset(
    2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            ObservedAt = new DateTimeOffset(
    2026, 9, 2, 8, 0, 0, TimeSpan.Zero)
        };

        var policy = new ConservativeSecurityPolicy();

        var resolution = policy.Resolve([vendorClaim]);

        Assert.Equal("not-affected", resolution.Status);
        Assert.Equal(vendorClaim.Id, resolution.SelectedClaim?.Id);
    }

    [Fact]
    public void EmptyClaims_ReturnsUnresolved()
    {
        var policy = new ConservativeSecurityPolicy();

        var resolution = policy.Resolve([]);

        Assert.Equal("unresolved", resolution.Status);
        Assert.Null(resolution.SelectedClaim);
    }
}