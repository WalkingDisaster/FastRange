using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange;

public class RangeSearch<TObject, TIndex> : IRangeSearch<TObject, TIndex>
    where TIndex : IComparable
{
    private readonly IRangeCheckFactory _rangeCheckFactory;
    private readonly IRangeSearcher<TObject, TIndex> _engine;

    public RangeSearch(IRangeSearcherFactory rangeSearcherFactory, IRangeCheckFactory rangeCheckFactory)
    {
        _engine = rangeSearcherFactory.Create<TObject, TIndex>();
        _rangeCheckFactory = rangeCheckFactory;
    }

    public Task<CheckResult> CheckAsync(Action<IRangeCheck<TIndex>> check, CancellationToken cancellationToken)
    {
        var rangeCheck = _rangeCheckFactory.Create(_engine);
        check(rangeCheck);
        return rangeCheck.CheckAsync(cancellationToken);
    }

    public Task AddAsync(TObject toAdd, IRangeElement<TIndex> range, CancellationToken cancellationToken)
    {
        return _engine.AddAsync(range.Floor, range.Ceiling, toAdd, cancellationToken);
    }

    public async Task AddAsync(TObject toAdd, CancellationToken cancellationToken, params IRangeElement<TIndex>[] ranges)
    {
        foreach (var range in ranges)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _engine.AddAsync(range.Floor, range.Ceiling, toAdd, cancellationToken);
        }
    }

    public Task AddAsync(TObject toAdd, TIndex floor, TIndex ceiling, CancellationToken cancellationToken)
    {
        return AddAsync(toAdd, cancellationToken, new RangeElement<TIndex>(floor, ceiling));
    }

    public async Task AddAsync(TObject toAdd, CancellationToken cancellationToken, params (TIndex floor, TIndex ceiling)[] ranges)
    {
        foreach (var (floor, ceiling) in ranges)
        {
            await AddAsync(toAdd, cancellationToken, new RangeElement<TIndex>(floor, ceiling));
        }
    }

    public async Task<FindResult<TObject>> FindAsync(TIndex index, CancellationToken cancellationToken)
    {
        if (_engine.Statistics.TotalRanges == 0)
        {
            throw new InvalidOperationException("The searcher must have at least one range.");
        }
        var results = await _engine.FindAsync(index, cancellationToken);
        var values = results.Select(x => x.value).ToArray();
        return new FindResult<TObject>(values.Length > 0, values);
    }

    public async Task<FindOneResult<TObject>> FindOneAsync(TIndex index, CancellationToken cancellationToken)
    {
        if (_engine.Statistics.TotalRanges == 0)
        {
            throw new InvalidOperationException("The searcher must have at least one range.");
        }
        var results = (await _engine.FindAsync(index, cancellationToken)).ToArray();
        return !results.Any()
            ? new FindOneResult<TObject>(false, default)
            : new FindOneResult<TObject>(true, results.Single().value);
    }
}