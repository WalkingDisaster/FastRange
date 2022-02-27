using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastRange;

public interface IRangeCheckExecutor<out T> : IRangeCheck<T> where T : IComparable
{
    Task<CheckResult> CheckAsync(CancellationToken cancellationToken);
}