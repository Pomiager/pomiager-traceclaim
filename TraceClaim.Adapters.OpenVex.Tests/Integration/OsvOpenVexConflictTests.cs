using TraceClaim.Adapters.OpenVex.Adapters;
using TraceClaim.Adapters.OpenVex.Models;
using TraceClaim.Adapters.Osv.Adapters;
using TraceClaim.Adapters.Osv.Models;
using TraceClaim.Core.Services;

namespace TraceClaim.Adapters.OpenVex.Tests.Integration;

public class OsvOpenVexConflictTests
{
    [Fact]
    public void OsvAffectedAndVendorNotAffected_AreDetectedAsConflict()
    {
        // OSV reports the concrete package version as affected.
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

        // The software vendor later issues a VEX statement saying that
        // the same product version is not affected by the vulnerability.
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

        var observedAt = new DateTimeOffset(
            2026, 9, 4, 8, 0, 0, TimeSpan.Zero);

        var osvAdapter = new OsvClaimAdapter();
        var vexAdapter = new OpenVexClaimAdapter();

        var osvClaim = Assert.Single(
            osvAdapter.Convert(osvDocument, observedAt));

        var vexClaim = Assert.Single(
            vexAdapter.Convert(vexDocument, observedAt));

        var detector = new ClaimConflictDetector();

        var conflict = detector.AreConflicting(
            osvClaim,
            vexClaim);

        Assert.True(conflict);
    }
}