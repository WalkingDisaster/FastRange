using System.Collections.Generic;

namespace FastRange;

public readonly struct FindResult<T>
{
    public bool Found { get; }
    public IEnumerable<T> Values { get; }

    internal FindResult(bool found, IEnumerable<T> values)
    {
        Found = found;
        Values = values;
    }

    public void Deconstruct(out bool found, out IEnumerable<T> values)
    {
        found = Found;
        values = Values;
    }
}