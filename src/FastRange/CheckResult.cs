namespace FastRange;

public readonly struct CheckResult
{
    public bool Succeeded { get; }
    public bool? IsContiguous { get; }
    public bool HasOverlap { get; }

    internal CheckResult(bool succeeded, bool? isContiguous, bool hasOverlap)
    {
        Succeeded = succeeded;
        IsContiguous = isContiguous;
        HasOverlap = hasOverlap;
    }

    public void Deconstruct(out bool succeeded, out bool? isContiguous, out bool hasOverlap)
    {
        succeeded = Succeeded;
        isContiguous = IsContiguous;
        hasOverlap = HasOverlap;
    }
}