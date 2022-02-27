using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange;

public delegate bool DetermineContiguity<in T>(T current, T next) where T : IComparable;

public interface IRangeSearcherCheck<out T> where T : IComparable
{
    Task<(bool isContiguous, bool hasOverlap)> CheckAsync
    (
        DetermineContiguity<T> determineContiguity,
        CancellationToken cancellationToken
    );
}