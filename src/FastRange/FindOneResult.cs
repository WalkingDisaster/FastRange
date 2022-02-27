namespace FastRange;

public readonly struct FindOneResult<T>
{
    public bool Found { get; }
    public T? Value { get; }

    internal FindOneResult(bool found, T? value)
    {
        Found = found;
        Value = value;
    }

    public void Deconstruct(out bool found, out T? value)
    {
        found = Found;
        value = Value;
    }
}