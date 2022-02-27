using System;
using FluentAssertions;
using Xunit;

namespace FastRange.Specifications;

public class Given_a_range_element
{
    public class When_constructing
    {
        [Fact]
        public void Then_the_values_should_be_set()
        {
            // arrange
            var sut = new RangeElement<int>(1, 10);

            // assert
            sut.Floor.Should().Be(1);
            sut.Ceiling.Should().Be(10);
        }
    }

    public class When_the_values_are_equal
    {
        [Fact]
        public void Then_an_exception_should_be_thrown()
        {
            var action = () => new RangeElement<int>(1, 1);
            action.Should().Throw<ArgumentException>();
        }
    }

    public class When_the_values_are_inverted
    {
        [Fact]
        public void Then_an_exception_should_be_thrown()
        {
            var action = () => new RangeElement<int>(2, 1);
            action.Should().Throw<ArgumentException>();
        }
    }
}