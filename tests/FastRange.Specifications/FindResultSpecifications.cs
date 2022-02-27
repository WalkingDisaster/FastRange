using FluentAssertions;
using Xunit;

namespace FastRange.Specifications;

public class Given_a_FindResult_when_setting_the_values
{
    [InlineData(false)]
    [InlineData(true, "only this one")]
    [InlineData(true, "the first one", "the second one")]
    [Theory]
    public void Then_the_properties_should_be_set(bool found, params string[] values)
    {
        var result = new FindResult<string>(found, values);
        result.Found.Should().Be(found);
        result.Values.Should().BeEquivalentTo(values);
    }

    [InlineData(false)]
    [InlineData(true, "only this one")]
    [InlineData(true, "the first one", "the second one")]
    [Theory]
    public void Then_the_property_deconstructor_should_work(bool found, params string[] values)
    {
        var (actualFound, actualValues) = new FindResult<string>(found, values);
        actualFound.Should().Be(found);
        actualValues.Should().BeEquivalentTo(values);
    }
}