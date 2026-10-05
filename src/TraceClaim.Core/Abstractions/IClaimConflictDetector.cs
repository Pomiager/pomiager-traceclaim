namespace TraceClaim.Core.Abstractions;

using TraceClaim.Core.Claims;

public interface IClaimConflictDetector
{
    bool AreConflicting(SoftwareClaim left, SoftwareClaim right);
}