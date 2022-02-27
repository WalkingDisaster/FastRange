using FluentAssertions;
using Xunit;

namespace FastRange.Specifications;

public class Given_a_CheckResult_when_setting_values
{
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [Theory]
    public void Then_the_properties_should_be_set(bool succeeded, bool isContiguous, bool hasOverlap)
    {
        var sut = new CheckResult(succeeded, isContiguous, hasOverlap);
        sut.Succeeded.Should().Be(succeeded);
        sut.IsContiguous.Should().Be(isContiguous);
        sut.HasOverlap.Should().Be(hasOverlap);
    }

    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [Theory]
    public void Then_the_property_deconstructor_should_work(bool succeeded, bool isContiguous, bool hasOverlap)
    {
        var (actualSucceeded, actualIsContiguous, actualHasOverlap) =
            new CheckResult(succeeded, isContiguous, hasOverlap);
        actualSucceeded.Should().Be(succeeded);
        actualIsContiguous.Should().Be(isContiguous);
        actualHasOverlap.Should().Be(hasOverlap);
    }
}