using TraceClaim.Core.Claims;
using TraceClaim.Core.Trust;

namespace TraceClaim.Core.Tests.Trust;

public class VendorPreferredPolicyTests
{
    [Fact]
    public void ConflictingClaims_SelectsVendorClaim()
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

        var policy = new VendorPreferredPolicy(
            "https://vendor.example");

        var resolution = policy.Resolve(
            [osvClaim, vendorClaim]);

        Assert.Equal("not-affected", resolution.Status);
        Assert.Equal(
            vendorClaim.Id,
            resolution.SelectedClaim?.Id);

        Assert.Equal(
            "vendor-preferred",
            resolution.Policy);
    }

    [Fact]
    public void MissingVendorClaim_ReturnsUnresolved()
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

        var policy = new VendorPreferredPolicy(
            "https://vendor.example");

        var resolution = policy.Resolve([osvClaim]);

        Assert.Equal("unresolved", resolution.Status);
        Assert.Null(resolution.SelectedClaim);
    }

    [Fact]
    public void MultipleVendorClaims_SelectsMostRecent()
    {
        var oldClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",

            // The claim was originally issued by the vendor on September 1st.
            // IssuedAt represents the issuer-side chronology and is therefore
            // the timestamp used when determining which vendor claim is newer.
            IssuedAt = new DateTimeOffset(
        2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            // TraceClaim observed the claim after it had already been issued.
            // Keeping this timestamp separate allows ingestion chronology
            // to differ from the issuer's own claim chronology.
            ObservedAt = new DateTimeOffset(
        2026, 9, 2, 8, 0, 0, TimeSpan.Zero)
        };

        var newClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",

            // Five days later the vendor publishes a newer assertion
            // about the same package and vulnerability.
            IssuedAt = new DateTimeOffset(
         2026, 9, 5, 10, 0, 0, TimeSpan.Zero),

            // TraceClaim ingests the newer assertion shortly after publication.
            ObservedAt = new DateTimeOffset(
         2026, 9, 5, 12, 0, 0, TimeSpan.Zero)
        };

        var policy = new VendorPreferredPolicy(
            "https://vendor.example");

        var resolution = policy.Resolve(
            [oldClaim, newClaim]);

        Assert.Equal("not-affected", resolution.Status);
        Assert.Equal(
            newClaim.Id,
            resolution.SelectedClaim?.Id);
    }
}