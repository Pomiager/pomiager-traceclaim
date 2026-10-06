using TraceClaim.Core.Claims;
using TraceClaim.Core.Relations;
using TraceClaim.Core.Services;

namespace TraceClaim.Core.Tests.Lineage;

public class ClaimLineageBuilderTests
{
    [Fact]
    public void SameIssuerSameSubjectAndObject_CreatesSupersessionRelation()
    {
        var oldClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",

            // The vendor originally reported the package as affected.
            IssuedAt = new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            // TraceClaim discovered the advisory one day later.
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

            // The vendor later revised its statement.
            IssuedAt = new DateTimeOffset(
                2026, 9, 5, 10, 0, 0, TimeSpan.Zero),

            // The new advisory was ingested shortly afterwards.
            ObservedAt = new DateTimeOffset(
                2026, 9, 5, 12, 0, 0, TimeSpan.Zero)
        };

        var builder = new ClaimLineageBuilder();

        var relations = builder.Build(
            [oldClaim, newClaim]);

        var relation = Assert.Single(relations);

        Assert.Equal(
            ClaimRelationType.Supersedes,
            relation.Type);

        // The newer claim is the source because:
        // "newClaim supersedes oldClaim".
        Assert.Equal(
            newClaim.Id,
            relation.SourceClaimId);

        Assert.Equal(
            oldClaim.Id,
            relation.TargetClaimId);
    }

    [Fact]
    public void ClaimsObservedOutOfOrder_AreOrderedByIssuedAt()
    {
        var olderIssuerClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",

            // This claim was issued first...
            IssuedAt = new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            // ...but TraceClaim discovered it later.
            ObservedAt = new DateTimeOffset(
                2026, 9, 10, 10, 0, 0, TimeSpan.Zero)
        };

        var newerIssuerClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",

            // This claim was issued later...
            IssuedAt = new DateTimeOffset(
                2026, 9, 5, 10, 0, 0, TimeSpan.Zero),

            // ...but happened to be ingested before the older one.
            ObservedAt = new DateTimeOffset(
                2026, 9, 6, 10, 0, 0, TimeSpan.Zero)
        };

        var builder = new ClaimLineageBuilder();

        var relations = builder.Build(
            [olderIssuerClaim, newerIssuerClaim]);

        var relation = Assert.Single(relations);

        // The relation must follow issuer chronology, not ingestion chronology.
        Assert.Equal(
            newerIssuerClaim.Id,
            relation.SourceClaimId);

        Assert.Equal(
            olderIssuerClaim.Id,
            relation.TargetClaimId);
    }
    [Fact]
    public void DifferentIssuers_DoNotSupersedeEachOther()
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
                2026, 9, 1, 11, 0, 0, TimeSpan.Zero)
        };

        var vendorClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = "not-affected-by",
            Object = "CVE-2026-1234",
            Issuer = "https://vendor.example",
            IssuedAt = new DateTimeOffset(
                2026, 9, 2, 10, 0, 0, TimeSpan.Zero),
            ObservedAt = new DateTimeOffset(
                2026, 9, 2, 11, 0, 0, TimeSpan.Zero)
        };

        var builder = new ClaimLineageBuilder();

        var relations = builder.Build(
            [osvClaim, vendorClaim]);

        // Claims from independent authorities may conflict,
        // but they do not supersede one another.
        Assert.Empty(relations);
    }
}