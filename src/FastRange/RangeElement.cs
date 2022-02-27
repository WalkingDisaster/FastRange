using System;

namespace FastRange;

public readonly struct RangeElement<T> : IRangeElement<T>
    where T : IComparable
{
    public T Floor { get; }
    public T Ceiling { get; }

    public RangeElement(T floor, T ceiling)
    {
        var comparison = floor.CompareTo(ceiling);
        if (comparison == 0)
        {
            throw new ArgumentException("The floor and ceiling of a range cannot be equal", nameof(ceiling));
        }

        if (comparison > 0)
        {
            throw new ArgumentException("The floor cannot be greater than the ceiling in a range.", nameof(ceiling));
        }

        Floor = floor;
        Ceiling = ceiling;
    }
}