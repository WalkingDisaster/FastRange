using System;

namespace FastRange;

public interface IRangeElement<out T>
    where T : IComparable
{
    T Floor { get; }
    T Ceiling { get; }
}