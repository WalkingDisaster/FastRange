using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange;

public interface IRangeSearcher<TObject, TIndex> : IRangeSearcherCheck<TIndex>
    where TIndex : IComparable
{
    IRangeSearcherStatistics Statistics { get; }
    Task AddAsync(TIndex floor, TIndex ceiling, TObject toAdd, CancellationToken cancellationToken);
    Task<IEnumerable<(TIndex floor, TIndex ceiling, TObject value)>> FindAsync(TIndex index, CancellationToken cancellationToken);
}