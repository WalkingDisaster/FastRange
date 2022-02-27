using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastRange.Specifications;

public class Given_a_RangeCheck
{
    public static readonly object[][] SharedTestData =
    {
        new object[] { true, true },
        new object[] { true, false },
        new object[] { false, true },
        new object[] { false, false },
    };

    private static IRangeSearcherCheck<int> GetIRangeSearcherCheck(bool isContiguous, bool hasOverlap)
    {
        var mock = new Mock<IRangeSearcherCheck<int>>();
        mock.Setup(x => x.CheckAsync(It.IsAny<DetermineContiguity<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((isContiguous, hasOverlap));
        return mock.Object;
    }

    public static RangeCheck<int> CreateSut(bool isContiguous, bool hasOverlap)
    {
        return new RangeCheck<int>(GetIRangeSearcherCheck(isContiguous, hasOverlap));
    }

    public class When_populating_values
    {
        public static object[][] TestData => SharedTestData;

        [MemberData(nameof(TestData))]
        [Theory]
        public async Task Then_the_output_values_must_be_the_same(bool isContiguous, bool hasOverlap)
        {
            // arrange
            var sut = CreateSut(isContiguous, hasOverlap);

            // act
            var result = await sut.CheckAsync(CancellationToken.None);

            // assert
            result.Succeeded.Should().BeTrue();
            result.IsContiguous.Should().BeNull();
            result.HasOverlap.Should().Be(hasOverlap);
        }
    }

    public class When_the_ranges_must_be_contiguous
    {
        public static object[][] TestData => SharedTestData;

        [MemberData(nameof(TestData))]
        [Theory]
        public async Task Then_the_CheckResult_produces_the_correct_result(bool isContiguous, bool hasOverlap)
        {
            // arrange
            var sut = CreateSut(isContiguous, hasOverlap);
            sut.RequireContiguous((_, _) => isContiguous);

            // act
            var result = await sut.CheckAsync(CancellationToken.None);

            // assert
            result.Succeeded.Should().Be(isContiguous);
        }
    }

    public class When_the_ranges_can_have_overlap
    {
        public static object[][] TestData => SharedTestData;

        [MemberData(nameof(TestData))]
        [Theory]
        public async Task Then_the_CheckResult_produces_the_correct_result(bool isContiguous, bool hasOverlap)
        {
            // arrange
            var sut = CreateSut(isContiguous, hasOverlap);

            // act
            var result = await sut.CheckAsync(CancellationToken.None);

            // assert
            result.Succeeded.Should().BeTrue();
        }
    }

    public class When_the_ranges_cannot_have_overlap
    {
        public static object[][] TestData => SharedTestData;

        [MemberData(nameof(TestData))]
        [Theory]
        public async Task Then_the_CheckResult_produces_the_correct_result(bool isContiguous, bool hasOverlap)
        {
            // arrange
            var sut = CreateSut(isContiguous, hasOverlap);
            sut.PreventOverlap();

            // act
            var result = await sut.CheckAsync(CancellationToken.None);

            // assert
            result.Succeeded.Should().Be(!hasOverlap);
        }
    }
}