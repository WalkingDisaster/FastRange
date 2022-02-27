using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange.Engines;

public class InMemoryRangeSearcher<TObject, TIndex> : IRangeSearcher<TObject, TIndex>, IRangeSearcherStatistics
    where TIndex : IComparable
{
    private readonly LinkedList<(TIndex floor, TIndex ceiling, TObject value)> _values;

    public InMemoryRangeSearcher()
    {
        _values = new LinkedList<(TIndex floor, TIndex ceiling, TObject value)>();
    }

    public Task<(bool isContiguous, bool hasOverlap)> CheckAsync(DetermineContiguity<TIndex> determineContiguity,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            if (_values.Count == 0) return (false, false);

            var isContiguous = true;
            var hasOverlap = false;

            using var enumerator = _values.GetEnumerator();
            var isFirst = true;
            var lastCeiling = _values.First.Value.ceiling;

            while (enumerator.MoveNext())
            {
                if (isFirst)
                {
                    lastCeiling = enumerator.Current.ceiling;
                    isFirst = false;
                    continue;
                }

                var (currentFloor, currentCeiling, _) = enumerator.Current;
                var contiguousWithLastRange = determineContiguity(lastCeiling, currentFloor);
                isContiguous &= contiguousWithLastRange;
                if (!(isContiguous || hasOverlap))
                {
                    var overlapsWithLastRange = lastCeiling.CompareTo(currentFloor) >= 0;
                    hasOverlap |= overlapsWithLastRange;
                }

                (_, lastCeiling) = (currentFloor, currentCeiling);
            }

            return (isContiguous, hasOverlap);
        }, cancellationToken);
    }

    IRangeSearcherStatistics IRangeSearcher<TObject, TIndex>.Statistics => this;

    Task IRangeSearcher<TObject, TIndex>.AddAsync(TIndex floor, TIndex ceiling, TObject toAdd,
        CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            if (!_values.Any())
            {
                _values.AddFirst((floor, ceiling, toAdd));
                return;
            }

            var lastNode = _values.First;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var floorCompare = floor.CompareTo(lastNode.Value.floor);
                if (floorCompare < 0)
                {
                    _values.AddBefore(lastNode, (floor, ceiling, toAdd));
                    return;
                }

                if (floorCompare == 0)
                {
                    var ceilingCompare = ceiling.CompareTo(lastNode.Value.ceiling);
                    if (ceilingCompare < 0)
                    {
                        _values.AddBefore(lastNode, (floor, ceiling, toAdd));
                        return;
                    }
                    else
                    {
                        var skipAheadNode = lastNode.Next;
                        while (skipAheadNode != null)
                        {
                            var skipAheadFloorCompare = floor.CompareTo(skipAheadNode.Value.floor);
                            if (skipAheadFloorCompare == 0)
                            {
                                var skipAheadCeilingCompare = ceiling.CompareTo(skipAheadNode.Value.ceiling);
                                if (skipAheadCeilingCompare < 0)
                                {
                                    _values.AddBefore(skipAheadNode, (floor, ceiling, toAdd));
                                    return;
                                }
                            }

                            skipAheadNode = skipAheadNode.Next;
                        }

                        _values.AddLast((floor, ceiling, toAdd));
                        return;
                    }
                }

                lastNode = lastNode.Next;
            } while (lastNode != null);

            _values.AddLast((floor, ceiling, toAdd));
        }, cancellationToken);
    }

    Task<IEnumerable<(TIndex floor, TIndex ceiling, TObject value)>> IRangeSearcher<TObject, TIndex>.FindAsync(
        TIndex index, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var result = new List<(TIndex, TIndex, TObject)>();
            using var enumerator = _values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var (floor, ceiling, _) = enumerator.Current;
                if (index.CompareTo(floor) >= 0 && index.CompareTo(ceiling) <= 0) result.Add(enumerator.Current);
            }

            return result.AsEnumerable();
        }, cancellationToken);
    }

    int IRangeSearcherStatistics.TotalRanges => _values.Count;
}