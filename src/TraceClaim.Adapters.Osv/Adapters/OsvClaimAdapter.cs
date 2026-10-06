using TraceClaim.Adapters.Osv.Models;
using TraceClaim.Core.Abstractions;
using TraceClaim.Core.Claims;

namespace TraceClaim.Adapters.Osv.Adapters;

/// <summary>
/// Converts OSV vulnerability metadata into normalized TraceClaim claims.
///
/// The adapter deliberately preserves the meaning of the source:
/// an entry inside OSV's "affected" collection becomes an "affected-by"
/// assertion in TraceClaim.
///
/// The adapter does NOT:
/// - decide whether OSV is trustworthy;
/// - compare OSV with other vulnerability sources;
/// - resolve conflicts;
/// - interpret OSV version ranges yet.
///
/// Those responsibilities belong to other TraceClaim components.
/// </summary>
public sealed class OsvClaimAdapter : IClaimAdapter<OsvDocument>
{
    /// <summary>
    /// Logical issuer assigned to claims imported from OSV.
    ///
    /// At this stage we identify the aggregation source itself.
    /// Later versions may preserve the original upstream database
    /// identity separately when available.
    /// </summary>
    public const string Issuer = "https://osv.dev";

    public IReadOnlyCollection<SoftwareClaim> Convert(
        OsvDocument source,
        DateTimeOffset observedAt)
    {
        ArgumentNullException.ThrowIfNull(source);

        var claims = new List<SoftwareClaim>();

        foreach (var affected in source.Affected)
        {
            if (affected.Package is null)
            {
                // Malformed source entries should not make the entire
                // advisory impossible to process.
                //
                // Input validation and diagnostic reporting will later
                // become a dedicated concern of the ingestion pipeline.
                continue;
            }

            foreach (var version in affected.Versions)
            {
                if (string.IsNullOrWhiteSpace(version))
                {
                    continue;
                }

                var subject = BuildVersionedPurl(
                    affected.Package,
                    version);

                claims.Add(new SoftwareClaim
                {
                    Id = Guid.NewGuid(),

                    // A TraceClaim subject identifies the concrete package
                    // version to which the assertion applies.
                    Subject = subject,

                    // OSV's affected[] semantics state that the listed package
                    // versions contain the vulnerability represented by
                    // the advisory.
                    Predicate = "affected-by",

                    // The OSV advisory identifier becomes the object of
                    // the normalized assertion.
                    Object = source.Id,

                    Issuer = Issuer,

                    // Published belongs to the source advisory and therefore
                    // maps to issuer-side chronology.
                    //
                    // Some external documents may omit a publication time.
                    // For this initial adapter we fall back to observation
                    // time rather than inventing an arbitrary timestamp.
                    //
                    // We will later make missing temporal information explicit
                    // in the claim model instead of relying on this fallback.
                    IssuedAt = source.Published ?? observedAt,

                    // ObservedAt always records when TraceClaim processed
                    // this particular external document.
                    ObservedAt = observedAt
                });
            }
        }

        return claims;
    }

    /// <summary>
    /// Produces a version-specific PURL for the affected package.
    ///
    /// OSV's optional package.purl normally identifies the package without
    /// a version. TraceClaim claims, however, need a concrete subject when
    /// converting explicit OSV versions.
    /// </summary>
    private static string BuildVersionedPurl(
        OsvPackage package,
        string version)
    {
        if (!string.IsNullOrWhiteSpace(package.Purl))
        {
            // The OSV specification recommends that package.purl does not
            // include @version. We append the explicit affected version here.
            //
            // A future PURL component will perform proper parsing and
            // canonicalisation instead of direct string manipulation.
            return $"{package.Purl}@{version}";
        }

        // Some OSV documents do not provide a PURL.
        //
        // For the first prototype we support a small set of well-known
        // ecosystems and construct a valid Package URL ourselves.
        // This mapping will later move to a reusable package identity service.
        var purlType = package.Ecosystem.ToLowerInvariant() switch
        {
            "nuget" => "nuget",
            "npm" => "npm",
            "pypi" => "pypi",
            "maven" => "maven",

            _ => throw new NotSupportedException(
                $"OSV ecosystem '{package.Ecosystem}' cannot currently be converted to PURL.")
        };

        return $"pkg:{purlType}/{package.Name}@{version}";
    }
}