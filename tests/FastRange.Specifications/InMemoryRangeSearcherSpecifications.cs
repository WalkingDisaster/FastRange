using FastRange.CommonSpecs;
using FastRange.Engines;

namespace FastRange.Specifications;

public class Given_an_InMemoryRangeSearcherSpecifications_instance : RangeSearcherSpecifications
{
    protected override IRangeSearcher<string, int> CreateSut()
    {
        return new InMemoryRangeSearcher<string, int>();
    }
}