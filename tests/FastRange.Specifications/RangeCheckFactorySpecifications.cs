using FluentAssertions;
using Moq;
using Xunit;

namespace FastRange.Specifications;

public class Given_a_RangeCheckFactory
{
    public class When_creating
    {
        [Fact]
        public void Then_it_should_return_a_RangeCheck()
        {
            var engine = Mock.Of<IRangeSearcher<string, int>>();
            var sut = new RangeCheckFactory();

            var result = sut.Create(engine);

            result.Should().NotBeNull();
        }
    }
}