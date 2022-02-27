using System;

namespace FastRange;

public interface IRangeSearcherFactory
{
    IRangeSearcher<TObject, TIndex> Create<TObject, TIndex>() where TIndex : IComparable;
}