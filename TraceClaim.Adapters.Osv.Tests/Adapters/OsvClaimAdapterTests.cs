using TraceClaim.Adapters.Osv.Adapters;
using TraceClaim.Adapters.Osv.Models;
using System.Text.Json;

namespace TraceClaim.Adapters.Osv.Tests.Adapters;

public class OsvClaimAdapterTests
{
    [Fact]
    public void Convert_ExplicitAffectedVersion_CreatesAffectedByClaim()
    {
        var document = new OsvDocument
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

        var observedAt = new DateTimeOffset(
            2026, 9, 2, 8, 30, 0, TimeSpan.Zero);

        var adapter = new OsvClaimAdapter();

        var claims = adapter.Convert(
            document,
            observedAt);

        var claim = Assert.Single(claims);

        Assert.Equal(
            "pkg:nuget/Acme.Security@2.1.0",
            claim.Subject);

        Assert.Equal(
            "affected-by",
            claim.Predicate);

        Assert.Equal(
            "CVE-2026-1234",
            claim.Object);

        Assert.Equal(
            OsvClaimAdapter.Issuer,
            claim.Issuer);

        // IssuedAt must represent the advisory's own publication time,
        // not the moment at which TraceClaim happened to ingest it.
        Assert.Equal(
            document.Published,
            claim.IssuedAt);

        // ObservedAt belongs to TraceClaim's ingestion history.
        Assert.Equal(
            observedAt,
            claim.ObservedAt);
    }

    [Fact]
    public void Convert_MultipleAffectedVersions_CreatesOneClaimPerVersion()
    {
        var document = new OsvDocument
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
                    "2.0.0",
                    "2.0.1",
                    "2.1.0"
                ]
            }
            ]
        };

        var adapter = new OsvClaimAdapter();

        var claims = adapter.Convert(
            document,
            new DateTimeOffset(
                2026, 9, 2, 8, 30, 0, TimeSpan.Zero));

        // Each explicit affected version becomes an independent normalized
        // assertion. This makes the resulting claims directly addressable
        // by version-specific PURL.
        Assert.Equal(3, claims.Count);

        Assert.Contains(
            claims,
            c => c.Subject == "pkg:nuget/Acme.Security@2.0.0");

        Assert.Contains(
            claims,
            c => c.Subject == "pkg:nuget/Acme.Security@2.0.1");

        Assert.Contains(
            claims,
            c => c.Subject == "pkg:nuget/Acme.Security@2.1.0");
    }
    [Fact]
    public void Convert_MissingPurl_BuildsPurlFromEcosystemAndName()
    {
        var document = new OsvDocument
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
                    Name = "Acme.Security"
                },

                Versions =
                [
                    "2.1.0"
                ]
            }
            ]
        };

        var adapter = new OsvClaimAdapter();

        var claims = adapter.Convert(
            document,
            new DateTimeOffset(
                2026, 9, 2, 8, 30, 0, TimeSpan.Zero));

        var claim = Assert.Single(claims);

        Assert.Equal(
            "pkg:nuget/Acme.Security@2.1.0",
            claim.Subject);
    }
    [Fact]
    public async Task OsvJson_CanBeDeserializedAndConvertedToClaims()
    {
        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Samples",
            "osv-explicit-versions.json");

        var json = await File.ReadAllTextAsync(filePath);

        var document = JsonSerializer.Deserialize<OsvDocument>(
            json);

        Assert.NotNull(document);

        var adapter = new OsvClaimAdapter();

        var claims = adapter.Convert(
            document,
            new DateTimeOffset(
                2026, 9, 4, 8, 0, 0, TimeSpan.Zero));

        Assert.Equal(3, claims.Count);

        Assert.All(
            claims,
            claim =>
            {
                Assert.Equal(
                    "CVE-2026-1234",
                    claim.Object);

                Assert.Equal(
                    "affected-by",
                    claim.Predicate);
            });
    }
}