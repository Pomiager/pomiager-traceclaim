using TraceClaim.Core.Claims;

namespace TraceClaim.Core.Abstractions;

/// <summary>
/// Converts a source-specific metadata document into one or more
/// normalized TraceClaim assertions.
///
/// The generic source type allows adapters to keep their external
/// representation outside TraceClaim.Core.
///
/// Examples:
/// OsvDocument     -> SoftwareClaim[]
/// OpenVexDocument -> SoftwareClaim[]
/// SlsaStatement   -> SoftwareClaim[]
/// </summary>
/// <typeparam name="TSource">
/// Source-specific document type consumed by the adapter.
/// </typeparam>
public interface IClaimAdapter<in TSource>
{
    /// <summary>
    /// Converts the supplied source document into normalized software claims.
    ///
    /// Adapters must preserve source semantics as faithfully as possible.
    /// They must not apply trust policies or silently resolve conflicts.
    /// </summary>
    IReadOnlyCollection<SoftwareClaim> Convert(
        TSource source,
        DateTimeOffset observedAt);
}