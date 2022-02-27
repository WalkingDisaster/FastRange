using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastRange.Specifications;

public class Give_a_RangeSearch
{
    public class When_checking_for_compliance
    {
        [Fact]
        public async Task Then_the_range_check_should_be_invoked()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var rangeCheckExecutorMock = new Mock<IRangeCheckExecutor<int>>();
            var rangeCheckFactoryMock = new Mock<IRangeCheckFactory>();
            var checkResult = new CheckResult(true, true, true);

            engineMock.Setup(x => x.CheckAsync(It.IsAny<DetermineContiguity<int>>(), It.IsAny<CancellationToken>())).ReturnsAsync((true, false));
            rangeCheckFactoryMock
                .Setup(x => x.Create(It.Is<IRangeSearcher<string, int>>(y => y == engineMock.Object)))
                .Returns(rangeCheckExecutorMock.Object);
            rangeCheckExecutorMock.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(checkResult);
            rangeCheckExecutorMock.Setup(x => x.PreventOverlap()).Returns(rangeCheckExecutorMock.Object);
            rangeCheckExecutorMock.Setup(x => x.RequireContiguous(It.IsAny<DetermineContiguity<int>>()))
                .Returns(rangeCheckExecutorMock.Object);
            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, rangeCheckFactoryMock.Object);

            // act
            var result = await sut.CheckAsync(x => x.PreventOverlap().RequireContiguous((_, _) => true), CancellationToken.None);

            // assert
            result.Should().Be(checkResult);
        }
    }

    public class When_adding_a_range
    {
        [Fact]
        public async Task Then_adding_a_single_IRangeElement_with_object_will_add_it_to_the_engine()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            var range = new RangeElement<int>(100, 1000);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            await sut.AddAsync("bubbles", range, CancellationToken.None);

            // assert
            engineMock.Verify(x => x.AddAsync(100, 1000, "bubbles", It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Then_adding_multiple_IRangeElement_with_object_will_add_it_to_the_engine()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            var range = new RangeElement<int>(100, 1000);
            var range2 = new RangeElement<int>(10000, 100000);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            await sut.AddAsync("chuckles", CancellationToken.None, range, range2);

            // assert
            engineMock.Verify(x => x.AddAsync(100, 1000, "chuckles", It.IsAny<CancellationToken>()));
            engineMock.Verify(x => x.AddAsync(10000, 100000, "chuckles", It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Then_adding_a_range_without_IRangeElement_will_add_it_to_the_engine()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            await sut.AddAsync("whizzle", 2, 4, CancellationToken.None);

            // assert
            engineMock.Verify(x => x.AddAsync(2, 4, "whizzle", It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task Then_adding_multiple_ranges_without_IRangeElement_will_add_it_to_the_engine()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            await sut.AddAsync("whump", CancellationToken.None, (99, 101), (102, 999));

            // assert
            engineMock.Verify(x => x.AddAsync(99, 101, "whump", CancellationToken.None));
            engineMock.Verify(x => x.AddAsync(102, 999, "whump", CancellationToken.None));
        }

        [InlineData(10, 10, "range values cannot be equal")]
        [InlineData(10, 9, "floor value must be greater than ceiling")]
        [Theory]
        public async Task Then_adding_invalid_ranges_will_fail(int floor, int ceiling, string because)
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var action = async () => await sut.AddAsync("bahrump", floor, ceiling, CancellationToken.None);

            // assert
            await action.Should().ThrowAsync<ArgumentException>(because);
        }

        [InlineData(10, 10, "range values cannot be equal")]
        [InlineData(10, 9, "floor value must be greater than ceiling")]
        [Theory]
        public async Task Then_adding_multiple_ranges_where_one_is_invalid_will_fail(int floor, int ceiling,
            string because)
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var action = async () =>
                await sut.AddAsync("Magwi", CancellationToken.None, (0, 1), (floor, ceiling), (100, 103));

            // assert
            await action.Should().ThrowAsync<ArgumentException>(because);
        }
    }

    public class When_no_ranges_have_been_added
    {
        [Fact]
        public async Task Then_Find_will_fail()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(0);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var action = async () => await sut.FindAsync(15, CancellationToken.None);

            // assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Then_FindOne_will_fail()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(0);

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var action = async () => await sut.FindOneAsync(15, CancellationToken.None);

            // assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    public class When_finding_a_range_with_multiple_results
    {
        [Fact]
        public async Task Then_Find_returns_successful()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(3);
            engineMock
                .Setup(x => x.FindAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    (0, 20, "wahahaa"),
                    (5, 25, "auauaua"),
                    (10, 30, "iurreiie")
                });

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var result = await sut.FindAsync(15, CancellationToken.None);

            // assert
            result.Found.Should().BeTrue();
            result.Values.Should().BeEquivalentTo(new[]
            {
                "wahahaa",
                "auauaua",
                "iurreiie"
            });
        }

        [Fact]
        public async Task Then_FindOne_is_unsuccessful()
        {
            // catch InvalidOperationException
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(3);
            engineMock
                .Setup(x => x.FindAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    (0, 20, "aeiou"),
                    (5, 25, "zyxwvut"),
                    (10, 30, "srqponmlk")
                });

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var action = async () => await sut.FindOneAsync(15, CancellationToken.None);

            // assert
            await action.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    public class When_finding_a_range_with_a_single_result
    {
        [Fact]
        public async Task Then_Find_returns_successful()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(3);
            engineMock
                .Setup(x => x.FindAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    (0, 20, "jihgf")
                });

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var result = await sut.FindAsync(15, CancellationToken.None);

            // assert
            result.Found.Should().BeTrue();
            result.Values.Should().BeEquivalentTo(new[] { "jihgf" });
        }

        [Fact]
        public async Task Then_FindOne_is_successful()
        {
            // catch InvalidOperationException
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(3);
            engineMock
                .Setup(x => x.FindAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    (0, 20, "edcba")
                });

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var result = await sut.FindOneAsync(15, CancellationToken.None);

            // assert
            result.Found.Should().BeTrue();
            result.Value.Should().Be("edcba");
        }
    }

    public class When_finding_a_range_with_no_results
    {
        [Fact]
        public async Task Then_Find_returns_unsuccessful()
        {
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(3);
            engineMock
                .Setup(x => x.FindAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<(int, int, string)>());

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var result = await sut.FindAsync(15, CancellationToken.None);

            // assert
            result.Found.Should().BeFalse();
            result.Values.Should().BeEmpty();
        }

        [Fact]
        public async Task Then_FindOne_is_unsuccessful()
        {
            // catch InvalidOperationException
            // arrange
            var engineMock = new Mock<IRangeSearcher<string, int>>();
            var rangeSearcherFactoryMock = new Mock<IRangeSearcherFactory>();
            var statsMock = new Mock<IRangeSearcherStatistics>();

            rangeSearcherFactoryMock
                .Setup(x => x.Create<string, int>())
                .Returns(engineMock.Object);
            engineMock.Setup(x => x.Statistics).Returns(statsMock.Object);
            statsMock.Setup(x => x.TotalRanges).Returns(3);
            engineMock
                .Setup(x => x.FindAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<(int, int, string)>());

            var sut = new RangeSearch<string, int>(rangeSearcherFactoryMock.Object, Mock.Of<IRangeCheckFactory>());

            // act
            var result = await sut.FindOneAsync(15, CancellationToken.None);

            // assert
            result.Found.Should().BeFalse();
            result.Value.Should().BeNull();
        }
    }
}