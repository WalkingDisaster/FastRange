using FluentAssertions;
using Xunit;

namespace FastRange.Specifications;

public class Given_a_FindOneResult_when_setting_values
{
    [InlineData(false, null)]
    [InlineData(true, "hiya")]
    [Theory]
    public void Then_the_values_should_be_set(bool found, string? value)
    {
        var sut = new FindOneResult<string>(found, value);
        sut.Found.Should().Be(found);
        sut.Value.Should().Be(value);
    }

    [InlineData(false, null)]
    [InlineData(true, "hiya")]
    [Theory]
    public void Then_the_property_deconstructor_should_work(bool found, string? value)
    {
        var (actualFound, actualValue) = new FindOneResult<string>(found, value);
        actualFound.Should().Be(found);
        actualValue.Should().Be(value);
    }
}