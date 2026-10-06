using TraceClaim.Adapters.OpenVex.Adapters;
using TraceClaim.Adapters.OpenVex.Models;
using System.Text.Json;

namespace TraceClaim.Adapters.OpenVex.Tests.Adapters;

public class OpenVexClaimAdapterTests
{
    [Fact]
    public void Convert_NotAffectedStatement_CreatesNotAffectedByClaim()
    {
        var document = new OpenVexDocument
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

                    // OpenVEX requires a justification or an impact statement
                    // for not_affected assertions. This synthetic example uses
                    // a standard machine-readable justification.
                    Justification = "vulnerable_code_not_in_execute_path"
                }
            ]
        };

        var observedAt = new DateTimeOffset(
            2026, 9, 4, 8, 0, 0, TimeSpan.Zero);

        var adapter = new OpenVexClaimAdapter();

        var claims = adapter.Convert(
            document,
            observedAt);

        var claim = Assert.Single(claims);

        Assert.Equal(
            "pkg:nuget/Acme.Security@2.1.0",
            claim.Subject);

        Assert.Equal(
            "not-affected-by",
            claim.Predicate);

        Assert.Equal(
            "CVE-2026-1234",
            claim.Object);

        Assert.Equal(
            "https://vendor.example",
            claim.Issuer);

        Assert.Equal(
            document.Timestamp,
            claim.IssuedAt);

        Assert.Equal(
            observedAt,
            claim.ObservedAt);
    }
    [Fact]
    public void Convert_StatementTimestamp_OverridesDocumentTimestamp()
    {
        var documentTimestamp = new DateTimeOffset(
            2026, 9, 1, 10, 0, 0, TimeSpan.Zero);

        var statementTimestamp = new DateTimeOffset(
            2026, 9, 5, 12, 0, 0, TimeSpan.Zero);

        var document = new OpenVexDocument
        {
            Author = "https://vendor.example",
            Timestamp = documentTimestamp,
            Version = 2,

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

                Status = "fixed",

                // A statement-level timestamp is more specific than the
                // enclosing document timestamp and must therefore win.
                Timestamp = statementTimestamp
            }
            ]
        };

        var adapter = new OpenVexClaimAdapter();

        var claims = adapter.Convert(
            document,
            new DateTimeOffset(
                2026, 9, 6, 8, 0, 0, TimeSpan.Zero));

        var claim = Assert.Single(claims);

        Assert.Equal(
            statementTimestamp,
            claim.IssuedAt);

        Assert.Equal(
            "fixed-for",
            claim.Predicate);
    }
    [Fact]
    public void Convert_MultipleProducts_CreatesOneClaimPerProduct()
    {
        var document = new OpenVexDocument
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
                    },
                    new OpenVexProduct
                    {
                        Id = "pkg:nuget/Acme.Security@2.1.1"
                    }
                ],

                Status = "affected"
            }
            ]
        };

        var adapter = new OpenVexClaimAdapter();

        var claims = adapter.Convert(
            document,
            new DateTimeOffset(
                2026, 9, 4, 8, 0, 0, TimeSpan.Zero));

        Assert.Equal(2, claims.Count);

        Assert.Contains(
            claims,
            c => c.Subject == "pkg:nuget/Acme.Security@2.1.0");

        Assert.Contains(
            claims,
            c => c.Subject == "pkg:nuget/Acme.Security@2.1.1");

        Assert.All(
            claims,
            c => Assert.Equal("affected-by", c.Predicate));
    }

    [Fact]
    public async Task OpenVexJson_CanBeDeserializedAndConvertedToClaim()
    {
        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Samples",
            "openvex-not-affected.json");

        var json = await File.ReadAllTextAsync(filePath);

        var document = JsonSerializer.Deserialize<OpenVexDocument>(
            json);

        Assert.NotNull(document);

        var adapter = new OpenVexClaimAdapter();

        var claims = adapter.Convert(
            document,
            new DateTimeOffset(
                2026, 9, 4, 8, 0, 0, TimeSpan.Zero));

        var claim = Assert.Single(claims);

        Assert.Equal(
            "not-affected-by",
            claim.Predicate);

        Assert.Equal(
            "CVE-2026-1234",
            claim.Object);

        Assert.Equal(
            "pkg:nuget/Acme.Security@2.1.0",
            claim.Subject);
    }
}