using System;

namespace FastRange;

public class RangeCheckFactory : IRangeCheckFactory
{
    public IRangeCheckExecutor<TIndex> Create<TObject, TIndex>(IRangeSearcher<TObject, TIndex> engine) where TIndex : IComparable
    {
        return new RangeCheck<TIndex>(engine);
    }
}