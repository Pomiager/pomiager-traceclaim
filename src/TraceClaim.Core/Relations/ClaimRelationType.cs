namespace TraceClaim.Core.Relations;

/// <summary>
/// Defines semantic relationships between software claims.
/// </summary>
public enum ClaimRelationType
{
    Supports,
    ConflictsWith,
    Supersedes
}