using System;

namespace FastRange;

public interface IRangeCheck<out T> where T : IComparable
{
    IRangeCheck<T> RequireContiguous(DetermineContiguity<T> determineContiguity);
    IRangeCheck<T> PreventOverlap();
}