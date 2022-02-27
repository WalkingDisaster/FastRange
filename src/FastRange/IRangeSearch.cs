using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange;

public interface IRangeSearch<TObject, TIndex>
    where TIndex : IComparable
{
    Task<CheckResult> CheckAsync(Action<IRangeCheck<TIndex>> check, CancellationToken cancellationToken);
    Task AddAsync(TObject toAdd, IRangeElement<TIndex> range, CancellationToken cancellationToken);
    Task AddAsync(TObject toAdd, CancellationToken cancellationToken, params IRangeElement<TIndex>[] ranges);
    Task AddAsync(TObject toAdd, TIndex floor, TIndex ceiling, CancellationToken cancellationToken);
    Task AddAsync(TObject toAdd, CancellationToken cancellationToken, params (TIndex floor, TIndex ceiling)[] ranges);
    Task<FindResult<TObject>> FindAsync(TIndex index, CancellationToken cancellationToken);
    Task<FindOneResult<TObject>> FindOneAsync(TIndex index, CancellationToken cancellationToken);
}