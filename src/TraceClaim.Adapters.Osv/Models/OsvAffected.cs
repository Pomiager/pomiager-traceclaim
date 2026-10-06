using System.Text.Json.Serialization;

namespace TraceClaim.Adapters.Osv.Models;

/// <summary>
/// Represents one affected-package entry inside an OSV advisory.
/// </summary>
public sealed class OsvAffected
{
    /// <summary>
    /// Identifies the affected software package.
    /// </summary>
    [JsonPropertyName("package")]
    public required OsvPackage Package { get; init; }

    /// <summary>
    /// Explicit package versions reported as affected.
    ///
    /// OSV also supports version ranges. TraceClaim v0.1 initially handles
    /// explicit versions only so that package-version semantics remain
    /// deterministic and easy to validate.
    /// </summary>
    [JsonPropertyName("versions")]
    public IReadOnlyCollection<string> Versions { get; init; } = [];
}