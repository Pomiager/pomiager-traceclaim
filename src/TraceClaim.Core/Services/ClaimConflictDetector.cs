using TraceClaim.Core.Abstractions;
using TraceClaim.Core.Claims;

namespace TraceClaim.Core.Services;

public sealed class ClaimConflictDetector : IClaimConflictDetector
{
    public bool AreConflicting(SoftwareClaim left, SoftwareClaim right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (!string.Equals(
                left.Subject,
                right.Subject,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(
                left.Object,
                right.Object,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return IsOppositePredicate(left.Predicate, right.Predicate);
    }

    private static bool IsOppositePredicate(
        string left,
        string right)
    {
        return
            IsPair(left, right, "affected-by", "not-affected-by");
    }

    private static bool IsPair(
        string left,
        string right,
        string first,
        string second)
    {
        return
            (string.Equals(left, first, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(right, second, StringComparison.OrdinalIgnoreCase))
            ||
            (string.Equals(left, second, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(right, first, StringComparison.OrdinalIgnoreCase));
    }
}