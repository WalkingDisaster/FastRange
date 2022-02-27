using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange;

public class RangeCheck<T> : IRangeCheckExecutor<T> where T : IComparable
{
    private readonly IRangeSearcherCheck<T> _check;
    private bool _requiresContiguous;
    private bool _allowsOverlap = true;
    private DetermineContiguity<T> _determineContiguity;

    public RangeCheck(IRangeSearcherCheck<T> check)
    {
        _check = check;
        _determineContiguity = (_, _) => false;
    }

    public async Task<CheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        var (isContiguous, hasOverlap) = await _check.CheckAsync(_determineContiguity, cancellationToken);
        var succeeded = ((!_requiresContiguous || isContiguous) && (_allowsOverlap || !hasOverlap));

        return new CheckResult(succeeded, _requiresContiguous ? isContiguous : null, hasOverlap);
    }

    public IRangeCheck<T> RequireContiguous(DetermineContiguity<T> determineContiguity)
    {
        _requiresContiguous = true;
        _determineContiguity = determineContiguity;
        return this;
    }

    public IRangeCheck<T> PreventOverlap()
    {
        _allowsOverlap = false;
        return this;
    }
}