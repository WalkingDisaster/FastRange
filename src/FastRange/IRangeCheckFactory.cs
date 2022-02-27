using System;

namespace FastRange;

public interface IRangeCheckFactory
{
    IRangeCheckExecutor<TIndex> Create<TObject, TIndex>(IRangeSearcher<TObject, TIndex> engine) where TIndex : IComparable;
}