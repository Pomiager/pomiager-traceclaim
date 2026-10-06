using System.Text.Json.Serialization;

namespace TraceClaim.Adapters.Osv.Models;

/// <summary>
/// Identifies a package according to the OSV schema.
/// </summary>
public sealed class OsvPackage
{
    /// <summary>
    /// Package ecosystem, such as NuGet, npm, PyPI or Maven.
    /// </summary>
    [JsonPropertyName("ecosystem")]
    public required string Ecosystem { get; init; }

    /// <summary>
    /// Package name as interpreted by the corresponding ecosystem.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional Package URL supplied directly by the OSV source.
    ///
    /// According to the OSV schema, this PURL normally identifies the
    /// package without an @version component.
    /// </summary>
    [JsonPropertyName("purl")]
    public string? Purl { get; init; }
}