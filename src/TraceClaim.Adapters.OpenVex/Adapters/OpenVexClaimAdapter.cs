using TraceClaim.Adapters.OpenVex.Models;
using TraceClaim.Core.Abstractions;
using TraceClaim.Core.Claims;

namespace TraceClaim.Adapters.OpenVex.Adapters;

/// <summary>
/// Converts OpenVEX statements into normalized TraceClaim claims.
///
/// The adapter preserves source semantics and does not attempt to decide
/// whether a VEX statement is more trustworthy than another metadata source.
///
/// Trust evaluation and conflict resolution belong to dedicated TraceClaim
/// policies and must remain independent from format conversion.
/// </summary>
public sealed class OpenVexClaimAdapter : IClaimAdapter<OpenVexDocument>
{
    public IReadOnlyCollection<SoftwareClaim> Convert(
        OpenVexDocument source,
        DateTimeOffset observedAt)
    {
        ArgumentNullException.ThrowIfNull(source);

        var claims = new List<SoftwareClaim>();

        foreach (var statement in source.Statements)
        {
            if (statement.Vulnerability is null)
            {
                // A malformed statement cannot be converted into a meaningful
                // TraceClaim assertion because the object of the claim is missing.
                //
                // Future ingestion infrastructure may report this condition as
                // a structured diagnostic rather than silently skipping it.
                continue;
            }

            var predicate = MapStatusToPredicate(statement.Status);

            var issuedAt = statement.Timestamp ?? source.Timestamp;

            foreach (var product in statement.Products)
            {
                if (string.IsNullOrWhiteSpace(product.Id))
                {
                    continue;
                }

                claims.Add(new SoftwareClaim
                {
                    Id = Guid.NewGuid(),

                    // OpenVEX commonly carries a PURL directly as product @id.
                    // TraceClaim preserves that identifier rather than attempting
                    // to reinterpret or rewrite it at this stage.
                    Subject = product.Id,

                    // The OpenVEX impact status is normalized to a TraceClaim
                    // predicate while retaining the original semantics.
                    Predicate = predicate,

                    // The vulnerability identifier becomes the object of
                    // the normalized software claim.
                    Object = statement.Vulnerability.Name,

                    // The author is the authority responsible for the VEX
                    // statement according to the OpenVEX document.
                    Issuer = source.Author,

                    // Statement timestamps override the enclosing document
                    // timestamp according to the OpenVEX inheritance model.
                    IssuedAt = issuedAt,

                    // ObservedAt records TraceClaim's own ingestion chronology.
                    ObservedAt = observedAt
                });
            }
        }

        return claims;
    }

    /// <summary>
    /// Maps OpenVEX status values into TraceClaim predicates.
    ///
    /// The mapping intentionally remains explicit so that unsupported or
    /// future OpenVEX statuses cannot silently acquire incorrect semantics.
    /// </summary>
    private static string MapStatusToPredicate(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException(
                "OpenVEX statement status cannot be empty.",
                nameof(status));
        }

        return status switch
        {
            "affected" => "affected-by",
            "not_affected" => "not-affected-by",
            "fixed" => "fixed-for",
            "under_investigation" => "under-investigation-for",

            _ => throw new NotSupportedException(
                $"OpenVEX status '{status}' is not currently supported.")
        };
    }
}