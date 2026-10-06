using TraceClaim.Adapters.OpenVex.Adapters;
using TraceClaim.Adapters.OpenVex.Models;
using TraceClaim.Adapters.Osv.Adapters;
using TraceClaim.Adapters.Osv.Models;
using TraceClaim.Core.Graph;
using TraceClaim.Core.Relations;
using TraceClaim.Core.Services;
using TraceClaim.Core.Trust;

namespace TraceClaim.Adapters.OpenVex.Tests.Integration;

/// <summary>
/// Demonstrates the first complete TraceClaim vertical slice.
///
/// The test intentionally starts from two independent software supply-chain
/// formats and ends with two different consumer-controlled resolutions.
///
/// The purpose is to prove that TraceClaim:
/// 1. preserves independent source assertions;
/// 2. detects explicit semantic conflicts;
/// 3. stores those conflicts in a common claim graph;
/// 4. keeps resolution separate from evidence;
/// 5. allows different trust policies to derive different decisions
///    from the exact same underlying claims.
/// </summary>
public class TraceClaimEndToEndTests
{
    [Fact]
    public void OsvAndOpenVexConflict_CanBeResolvedByDifferentTrustPolicies()
    {
        var observedAt = new DateTimeOffset(
            2026, 9, 4, 8, 0, 0, TimeSpan.Zero);

        // OSV states that version 2.1.0 of the package is affected
        // by CVE-2026-1234.
        var osvDocument = new OsvDocument
        {
            Id = "CVE-2026-1234",

            Published = new DateTimeOffset(
                2026, 9, 1, 10, 0, 0, TimeSpan.Zero),

            Affected =
            [
                new OsvAffected
                {
                    Package = new OsvPackage
                    {
                        Ecosystem = "NuGet",
                        Name = "Acme.Security",
                        Purl = "pkg:nuget/Acme.Security"
                    },

                    Versions =
                    [
                        "2.1.0"
                    ]
                }
            ]
        };

        // The vendor later publishes an OpenVEX assertion stating that
        // the exact same software version is not affected.
        var vexDocument = new OpenVexDocument
        {
            Author = "https://vendor.example",

            Timestamp = new DateTimeOffset(
                2026, 9, 3, 10, 0, 0, TimeSpan.Zero),

            Version = 1,

            Statements =
            [
                new OpenVexStatement
                {
                    Vulnerability = new OpenVexVulnerability
                    {
                        Name = "CVE-2026-1234"
                    },

                    Products =
                    [
                        new OpenVexProduct
                        {
                            Id = "pkg:nuget/Acme.Security@2.1.0"
                        }
                    ],

                    Status = "not_affected",

                    Justification =
                        "vulnerable_code_not_in_execute_path"
                }
            ]
        };

        var osvAdapter = new OsvClaimAdapter();
        var vexAdapter = new OpenVexClaimAdapter();

        var osvClaim = Assert.Single(
            osvAdapter.Convert(
                osvDocument,
                observedAt));

        var vexClaim = Assert.Single(
            vexAdapter.Convert(
                vexDocument,
                observedAt));

        // TraceClaim now has two independently sourced assertions.
        //
        // Neither claim is discarded, overwritten or treated as authoritative
        // merely because it was processed later.
        var graph = new ClaimGraph();

        graph.AddClaim(osvClaim);
        graph.AddClaim(vexClaim);

        var conflictDetector = new ClaimConflictDetector();

        Assert.True(
            conflictDetector.AreConflicting(
                osvClaim,
                vexClaim));

        // Conflict detection and graph storage are separate operations.
        //
        // The detector establishes that the claims are semantically
        // incompatible; the graph preserves that fact as an explicit edge.
        graph.AddRelation(new ClaimRelation
        {
            SourceClaimId = osvClaim.Id,
            TargetClaimId = vexClaim.Id,
            Type = ClaimRelationType.ConflictsWith,
            Reason =
                "OSV reports the package as affected while the vendor OpenVEX statement reports it as not affected."
        });

        var conflicts = graph.GetConflicts();

        Assert.Single(conflicts);

        // Policy 1:
        //
        // A conservative security consumer treats any credible "affected"
        // assertion as sufficient reason to consider the software affected.
        var conservativePolicy =
            new ConservativeSecurityPolicy();

        var conservativeResolution =
            conservativePolicy.Resolve(
                graph.GetCurrentClaims());

        Assert.Equal(
            "affected",
            conservativeResolution.Status);

        Assert.Equal(
            osvClaim.Id,
            conservativeResolution.SelectedClaim?.Id);

        // Policy 2:
        //
        // Another consumer may explicitly trust the software vendor as the
        // preferred authority for exploitability information.
        //
        // The same evidence graph therefore produces a different resolution
        // without changing or deleting either original claim.
        var vendorPreferredPolicy =
            new VendorPreferredPolicy(
                "https://vendor.example");

        var vendorResolution =
            vendorPreferredPolicy.Resolve(
                graph.GetCurrentClaims());

        Assert.Equal(
            "not-affected",
            vendorResolution.Status);

        Assert.Equal(
            vexClaim.Id,
            vendorResolution.SelectedClaim?.Id);

        // The important invariant:
        //
        // Resolution never mutates the evidence graph.
        Assert.Equal(
            2,
            graph.Claims.Count);

        Assert.Single(
            graph.GetConflicts());
    }
}