using TraceClaim.Core.Claims;
using TraceClaim.Core.Graph;
using TraceClaim.Core.Relations;

namespace TraceClaim.Core.Tests.Graph;

public class ClaimGraphTests
{
    [Fact]
    public void AddClaim_StoresClaim()
    {
        var claim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://osv.dev",
            issuedAt: new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var graph = new ClaimGraph();

        graph.AddClaim(claim);

        var storedClaim = graph.GetClaim(claim.Id);

        Assert.NotNull(storedClaim);
        Assert.Equal(claim.Id, storedClaim.Id);
    }

    private static SoftwareClaim CreateClaim(
        string predicate,
        string issuer,
        DateTimeOffset issuedAt)
    {
        return new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/acme.security@2.1.0",
            Predicate = predicate,
            Object = "CVE-2026-1234",
            Issuer = issuer,

            // IssuedAt belongs to the authority that created the claim.
            // It is used to reconstruct the logical history of assertions.
            IssuedAt = issuedAt,

            // In these graph tests the ingestion time is not the behaviour
            // under test, so we simply observe each claim one hour later.
            ObservedAt = issuedAt.AddHours(1)
        };
    }
    [Fact]
    public void AddClaim_DuplicateId_ThrowsException()
    {
        var claim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://osv.dev",
            issuedAt: new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var graph = new ClaimGraph();

        graph.AddClaim(claim);

        // A duplicate claim identifier would make graph traversal ambiguous,
        // therefore ClaimGraph rejects it explicitly.
        var exception = Assert.Throws<InvalidOperationException>(
            () => graph.AddClaim(claim));

        Assert.Contains(
            claim.Id.ToString(),
            exception.Message);
    }
    [Fact]
    public void AddRelation_MissingTargetClaim_ThrowsException()
    {
        var sourceClaim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://osv.dev",
            issuedAt: new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var graph = new ClaimGraph();
        graph.AddClaim(sourceClaim);

        var relation = new ClaimRelation
        {
            SourceClaimId = sourceClaim.Id,
            TargetClaimId = Guid.NewGuid(),
            Type = ClaimRelationType.ConflictsWith,
            Reason = "Test relationship."
        };

        // A graph must never contain an edge pointing to a node that does not
        // exist. Enforcing this rule here keeps the in-memory model consistent.
        Assert.Throws<InvalidOperationException>(
            () => graph.AddRelation(relation));
    }
    [Fact]
    public void GetCurrentClaims_ExcludesSupersededClaim()
    {
        var oldVendorClaim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://vendor.example",
            issuedAt: new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var newVendorClaim = CreateClaim(
            predicate: "not-affected-by",
            issuer: "https://vendor.example",
            issuedAt: new DateTimeOffset(
                2026, 9, 5, 10, 0, 0, TimeSpan.Zero));

        var graph = new ClaimGraph();

        graph.AddClaim(oldVendorClaim);
        graph.AddClaim(newVendorClaim);

        graph.AddRelation(new ClaimRelation
        {
            // The relation reads:
            //
            // newVendorClaim supersedes oldVendorClaim
            SourceClaimId = newVendorClaim.Id,
            TargetClaimId = oldVendorClaim.Id,
            Type = ClaimRelationType.Supersedes,
            Reason = "The vendor published a newer assertion."
        });

        var currentClaims = graph.GetCurrentClaims();

        Assert.Single(currentClaims);
        Assert.Contains(
            currentClaims,
            c => c.Id == newVendorClaim.Id);

        Assert.DoesNotContain(
            currentClaims,
            c => c.Id == oldVendorClaim.Id);
    }
    [Fact]
    public void GetCurrentClaims_PreservesIndependentAuthorities()
    {
        var osvClaim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://osv.dev",
            issuedAt: new DateTimeOffset(
                2026, 9, 2, 9, 0, 0, TimeSpan.Zero));

        var oldVendorClaim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://vendor.example",
            issuedAt: new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var newVendorClaim = CreateClaim(
            predicate: "not-affected-by",
            issuer: "https://vendor.example",
            issuedAt: new DateTimeOffset(
                2026, 9, 5, 10, 0, 0, TimeSpan.Zero));

        var graph = new ClaimGraph();

        graph.AddClaim(osvClaim);
        graph.AddClaim(oldVendorClaim);
        graph.AddClaim(newVendorClaim);

        graph.AddRelation(new ClaimRelation
        {
            SourceClaimId = newVendorClaim.Id,
            TargetClaimId = oldVendorClaim.Id,
            Type = ClaimRelationType.Supersedes,
            Reason = "The vendor revised its previous statement."
        });

        graph.AddRelation(new ClaimRelation
        {
            SourceClaimId = osvClaim.Id,
            TargetClaimId = newVendorClaim.Id,
            Type = ClaimRelationType.ConflictsWith,
            Reason =
                "OSV reports the package as affected while the vendor reports it as not affected."
        });

        var currentClaims = graph.GetCurrentClaims();

        // The old vendor statement is no longer current.
        Assert.DoesNotContain(
            currentClaims,
            c => c.Id == oldVendorClaim.Id);

        // The latest vendor statement remains current.
        Assert.Contains(
            currentClaims,
            c => c.Id == newVendorClaim.Id);

        // The independent OSV claim also remains current.
        //
        // A conflict between authorities does not imply supersession.
        Assert.Contains(
            currentClaims,
            c => c.Id == osvClaim.Id);

        Assert.Equal(2, currentClaims.Count);
    }

    [Fact]
    public void GetClaimsForSubject_ReturnsOnlyMatchingSubject()
    {
        var matchingClaim = CreateClaim(
            predicate: "affected-by",
            issuer: "https://osv.dev",
            issuedAt: new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero));

        var differentClaim = new SoftwareClaim
        {
            Id = Guid.NewGuid(),
            Subject = "pkg:nuget/another.package@1.0.0",
            Predicate = "affected-by",
            Object = "CVE-2026-9999",
            Issuer = "https://osv.dev",
            IssuedAt = new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero),
            ObservedAt = new DateTimeOffset(
                2026, 9, 1, 11, 0, 0, TimeSpan.Zero)
        };

        var graph = new ClaimGraph();

        graph.AddClaim(matchingClaim);
        graph.AddClaim(differentClaim);

        var claims = graph.GetClaimsForSubject(
            "pkg:nuget/acme.security@2.1.0");

        Assert.Single(claims);
        Assert.Equal(
            matchingClaim.Id,
            claims.Single().Id);
    }

}