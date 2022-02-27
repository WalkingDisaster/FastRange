using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace FastRange.CommonSpecs;

public abstract class RangeSearcherSpecifications
{
    protected abstract IRangeSearcher<string, int> CreateSut();

    private static async Task AddRanges(IRangeSearcher<string, int> engine, params (int, int)[] values)
    {
        foreach (var (floor, ceiling) in values)
        {
            var toAdd = $"{floor}-{ceiling}";
            await engine.AddAsync(floor, ceiling, toAdd, CancellationToken.None);
        }
    }

    private async Task<IRangeSearcher<string, int>> GetContiguousSut()
    {
        var sut = CreateSut();

        await AddRanges
        (
            sut,
            new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Select(x => (x * 10, (x * 10) + 9)).ToArray()
        );
        return sut;
    }

    private async Task<IRangeSearcher<string, int>> GetOverlappingSut()
    {
        var sut = CreateSut();

        await AddRanges
        (
            sut,
            new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Select(x => (x * 10, (x * 10) + 10)).ToArray()
        );
        return sut;
    }

    private async Task<IRangeSearcher<string, int>> GetNotContiguousNotOverlappingSut()
    {
        var sut = CreateSut();

        await AddRanges
        (
            sut,
            new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Select(x => (x * 10, (x * 10) + 8)).ToArray()
        );
        return sut;
    }

    private async Task<IRangeSearcher<string, int>> GetMixedBagSut()
    {
        var sut = CreateSut();

        await AddRanges
        (
            sut,
            (0, 9),
            (10, 20),
            (19, 30),
            (20, 29),
            (40, 49)
        );
        return sut;
    }

    private async Task<IRangeSearcher<string, int>> GetSutForAddTest()
    {
        var sut = CreateSut();

        await AddRanges
        (
            sut,
            (10, 19),
            (20, 29),
            (40, 49)
        );
        return sut;
    }

    [Fact]
    public async Task When_there_are_no_ranges_then_checking_should_return_defaults()
    {
        var sut = CreateSut();
        var (isContiguous, hasOverlap) = await sut.CheckAsync((_, _) => false, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeFalse();
    }

    [Fact]
    public async Task When_the_ranges_are_contiguous_then_the_statistics_should_be_correct()
    {
        var sut = await GetContiguousSut();
        sut.Statistics.TotalRanges.Should().Be(10);
    }

    [Fact]
    public async Task When_the_ranges_are_contiguous_then_the_check_results_should_be_correct()
    {
        var sut = await GetContiguousSut();
        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeTrue();
        hasOverlap.Should().BeFalse();
    }

    [InlineData(-1, 0)]
    [InlineData(0, 1, "0-9")]
    [InlineData(5, 1, "0-9")]
    [InlineData(9, 1, "0-9")]
    [InlineData(10, 1, "10-19")]
    [InlineData(11, 1, "10-19")]
    [InlineData(19, 1, "10-19")]
    [InlineData(20, 1, "20-29")]
    [InlineData(21, 1, "20-29")]
    [InlineData(29, 1, "20-29")]
    [InlineData(30, 1, "30-39")]
    [InlineData(32, 1, "30-39")]
    [InlineData(39, 1, "30-39")]
    [InlineData(40, 1, "40-49")]
    [InlineData(43, 1, "40-49")]
    [InlineData(49, 1, "40-49")]
    [InlineData(50, 1, "50-59")]
    [InlineData(54, 1, "50-59")]
    [InlineData(59, 1, "50-59")]
    [InlineData(60, 1, "60-69")]
    [InlineData(65, 1, "60-69")]
    [InlineData(69, 1, "60-69")]
    [InlineData(70, 1, "70-79")]
    [InlineData(76, 1, "70-79")]
    [InlineData(79, 1, "70-79")]
    [InlineData(80, 1, "80-89")]
    [InlineData(87, 1, "80-89")]
    [InlineData(89, 1, "80-89")]
    [InlineData(90, 1, "90-99")]
    [InlineData(98, 1, "90-99")]
    [InlineData(99, 1, "90-99")]
    [InlineData(100, 0)]
    [Theory]
    public async Task When_the_ranges_are_contiguous_then_getting_a_value_should_return_the_appropriate_value
    (
        int index,
        int expectedResultCount,
        params string[] expectedValues
    )
    {
        var sut = await GetContiguousSut();
        var results = (await sut.FindAsync(index, CancellationToken.None)).ToArray();

        results.Should().NotBeNull();
        results.Length.Should().Be(expectedResultCount);
        foreach (var expectedValue in expectedValues)
        {
            results.Any(x => x.value == expectedValue).Should().BeTrue();
        }
    }

    [Fact]
    public async Task When_the_ranges_are_overlapping_then_the_statistics_should_be_correct()
    {
        var sut = await GetOverlappingSut();
        sut.Statistics.TotalRanges.Should().Be(10);
    }

    [Fact]
    public async Task When_the_ranges_are_overlapping_then_the_check_results_should_be_correct()
    {
        var sut = await GetOverlappingSut();
        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeTrue();
    }

    [InlineData(-1, 0)]
    [InlineData(0, 1, "0-10")]
    [InlineData(5, 1, "0-10")]
    [InlineData(9, 1, "0-10")]
    [InlineData(10, 2, "0-10", "10-20")]
    [InlineData(11, 1, "10-20")]
    [InlineData(19, 1, "10-20")]
    [InlineData(20, 2, "10-20", "20-30")]
    [InlineData(21, 1, "20-30")]
    [InlineData(29, 1, "20-30")]
    [InlineData(30, 2, "20-30", "30-40")]
    [InlineData(32, 1, "30-40")]
    [InlineData(39, 1, "30-40")]
    [InlineData(40, 2, "30-40", "40-50")]
    [InlineData(43, 1, "40-50")]
    [InlineData(49, 1, "40-50")]
    [InlineData(50, 2, "40-50", "50-60")]
    [InlineData(54, 1, "50-60")]
    [InlineData(59, 1, "50-60")]
    [InlineData(60, 2, "50-60", "60-70")]
    [InlineData(65, 1, "60-70")]
    [InlineData(69, 1, "60-70")]
    [InlineData(70, 2, "60-70", "70-80")]
    [InlineData(76, 1, "70-80")]
    [InlineData(79, 1, "70-80")]
    [InlineData(80, 2, "70-80", "80-90")]
    [InlineData(87, 1, "80-90")]
    [InlineData(89, 1, "80-90")]
    [InlineData(90, 2, "80-90", "90-100")]
    [InlineData(98, 1, "90-100")]
    [InlineData(99, 1, "90-100")]
    [InlineData(100, 1, "90-100")]
    [InlineData(101, 0)]
    [Theory]
    public async Task When_the_ranges_are_overlapping_then_getting_a_value_should_return_the_appropriate_value
    (
        int index,
        int expectedResultCount,
        params string[] expectedValues
    )
    {
        var sut = CreateSut();

        await AddRanges
        (
            sut,
            new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Select(x => (x * 10, (x * 10) + 10)).ToArray()
        );

        var results = (await sut.FindAsync(index, CancellationToken.None)).ToArray();

        results.Should().NotBeNull();
        results.Length.Should().Be(expectedResultCount);
        foreach (var expectedValue in expectedValues)
        {
            results.Any(x => x.value == expectedValue).Should().BeTrue();
        }
    }

    [Fact]
    public async Task When_the_ranges_are_not_contiguous_or_overlapping_then_the_statistics_should_be_correct()
    {
        var sut = await GetNotContiguousNotOverlappingSut();
        sut.Statistics.TotalRanges.Should().Be(10);
    }

    [Fact]
    public async Task When_the_ranges_are_not_contiguous_or_overlapping_then_the_check_results_should_be_correct()
    {
        var sut = await GetNotContiguousNotOverlappingSut();
        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeFalse();
    }

    [InlineData(-1, 0)]
    [InlineData(0, 1, "0-8")]
    [InlineData(5, 1, "0-8")]
    [InlineData(8, 1, "0-8")]
    [InlineData(9, 0)]
    [InlineData(10, 1, "10-18")]
    [InlineData(11, 1, "10-18")]
    [InlineData(18, 1, "10-18")]
    [InlineData(19, 0)]
    [InlineData(20, 1, "20-28")]
    [InlineData(21, 1, "20-28")]
    [InlineData(28, 1, "20-28")]
    [InlineData(29, 0)]
    [InlineData(30, 1, "30-38")]
    [InlineData(32, 1, "30-38")]
    [InlineData(38, 1, "30-38")]
    [InlineData(39, 0)]
    [InlineData(40, 1, "40-48")]
    [InlineData(43, 1, "40-48")]
    [InlineData(48, 1, "40-48")]
    [InlineData(49, 0)]
    [InlineData(50, 1, "50-58")]
    [InlineData(54, 1, "50-58")]
    [InlineData(58, 1, "50-58")]
    [InlineData(59, 0)]
    [InlineData(60, 1, "60-68")]
    [InlineData(65, 1, "60-68")]
    [InlineData(68, 1, "60-68")]
    [InlineData(69, 0)]
    [InlineData(70, 1, "70-78")]
    [InlineData(76, 1, "70-78")]
    [InlineData(78, 1, "70-78")]
    [InlineData(79, 0)]
    [InlineData(80, 1, "80-88")]
    [InlineData(87, 1, "80-88")]
    [InlineData(88, 1, "80-88")]
    [InlineData(89, 0)]
    [InlineData(90, 1, "90-98")]
    [InlineData(97, 1, "90-98")]
    [InlineData(98, 1, "90-98")]
    [InlineData(99, 0)]
    [Theory]
    public async Task
        When_the_ranges_are_not_contiguous_or_overlapping_then_getting_a_value_should_return_the_appropriate_value
        (
            int index,
            int expectedResultCount,
            params string[] expectedValues
        )
    {
        var sut = await GetNotContiguousNotOverlappingSut();
        var results = (await sut.FindAsync(index, CancellationToken.None)).ToArray();

        results.Should().NotBeNull();
        results.Length.Should().Be(expectedResultCount);
        foreach (var expectedValue in expectedValues)
        {
            results.Any(x => x.value == expectedValue).Should().BeTrue();
        }
    }

    [Fact]
    public async Task
        When_the_ranges_are_a_have_some_non_contiguous_and_some_overlapping_then_the_statistics_should_be_correct()
    {
        var sut = await GetMixedBagSut();
        sut.Statistics.TotalRanges.Should().Be(5);
    }

    [Fact]
    public async Task
        When_the_ranges_have_some_non_contiguous_and_some_overlapping_then_the_check_results_should_be_correct()
    {
        var sut = await GetMixedBagSut();
        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeTrue();
    }

    [InlineData(-1, 0)]
    [InlineData(0, 1, "0-9")]
    [InlineData(9, 1, "0-9")]
    [InlineData(10, 1, "10-20")]
    [InlineData(19, 2, "10-20", "19-30")]
    [InlineData(20, 3, "10-20", "19-30", "20-29")]
    [InlineData(29, 2, "19-30", "20-29")]
    [InlineData(30, 1, "19-30")]
    [InlineData(31, 0)]
    [InlineData(39, 0)]
    [InlineData(40, 1, "40-49")]
    [InlineData(49, 1, "40-49")]
    [InlineData(50, 0)]
    [Theory]
    public async Task
        When_the_ranges_have_some_non_contiguous_and_some_overlapping_then_getting_a_value_should_return_the_appropriate_value
        (
            int index,
            int expectedResultCount,
            params string[] expectedValues
        )
    {
        var sut = await GetMixedBagSut();
        var results = (await sut.FindAsync(index, CancellationToken.None)).ToArray();

        results.Should().NotBeNull();
        results.Length.Should().Be(expectedResultCount);
        foreach (var expectedValue in expectedValues)
        {
            results.Any(x => x.value == expectedValue).Should().BeTrue();
        }
    }

    [Fact]
    public async Task When_adding_a_contiguous_value_at_the_beginning_then_it_should_be_added_properly()
    {
        var sut = await GetSutForAddTest();
        await sut.AddAsync(0, 9, "i added this", CancellationToken.None);

        var (isContiguous, hasOverlap) = await sut.CheckAsync((f, c) => f + 1 == c, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeFalse();

        var result = (await sut.FindAsync(5, CancellationToken.None)).ToArray();

        result.Should().NotBeNull();
        result.Length.Should().Be(1);
        var (floor, ceiling, value) = result.Single();
        floor.Should().Be(0);
        ceiling.Should().Be(9);
        value.Should().Be("i added this");
    }

    [Fact]
    public async Task When_adding_a_contiguous_value_at_the_beginning_with_overlap_then_it_should_be_added_properly()
    {
        var sut = await GetSutForAddTest();
        await sut.AddAsync(0, 10, "overlaps at beginning", CancellationToken.None);

        var (isContiguous, hasOverlap) = await sut.CheckAsync((f, c) => f + 1 == c, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeTrue();

        var result = (await sut.FindAsync(5, CancellationToken.None)).ToArray();

        result.Should().NotBeNull();
        result.Length.Should().Be(1);
        var (floor, ceiling, value) = result.Single();
        floor.Should().Be(0);
        ceiling.Should().Be(10);
        value.Should().Be("overlaps at beginning");
    }

    [Fact]
    public async Task When_adding_a_contiguous_value_at_the_end_then_it_should_be_added_properly()
    {
        var sut = await GetSutForAddTest();
        await sut.AddAsync(50, 59, "go to the end", CancellationToken.None);

        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeFalse();

        var result = (await sut.FindAsync(55, CancellationToken.None)).ToArray();

        result.Should().NotBeNull();
        result.Length.Should().Be(1);
        var (floor, ceiling, value) = result.Single();
        floor.Should().Be(50);
        ceiling.Should().Be(59);
        value.Should().Be("go to the end");
    }

    [Fact]
    public async Task When_adding_a_contiguous_value_overlaps_at_the_end_then_it_should_be_added_properly()
    {
        var sut = await GetSutForAddTest();
        await sut.AddAsync(49, 59, "overlaps at the end", CancellationToken.None);

        var (isContiguous, hasOverlap) = await sut.CheckAsync((f, c) => f + 1 == c, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeTrue();

        var result = (await sut.FindAsync(55, CancellationToken.None)).ToArray();

        result.Should().NotBeNull();
        result.Length.Should().Be(1);
        var (floor, ceiling, value) = result.Single();
        floor.Should().Be(49);
        ceiling.Should().Be(59);
        value.Should().Be("overlaps at the end");
    }

    [Fact]
    public async Task When_adding_a_value_that_makes_it_contiguous_then_the_check_should_return_contiguous()
    {
        var sut = await GetSutForAddTest();
        await sut.AddAsync(30, 39, "thirty to almost forty", CancellationToken.None);

        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeTrue();
        hasOverlap.Should().BeFalse();

        var result = (await sut.FindAsync(35, CancellationToken.None)).ToArray();

        result.Should().NotBeNull();
        result.Length.Should().Be(1);
        var (floor, ceiling, value) = result.Single();
        floor.Should().Be(30);
        ceiling.Should().Be(39);
        value.Should().Be("thirty to almost forty");
    }

    [Fact]
    public async Task
        When_adding_a_value_that_has_the_same_floor_and_is_contained_within_the_overlap_then_it_should_still_work()
    {
        var sut = await GetSutForAddTest();
        await sut.AddAsync(10, 15, "same start number", CancellationToken.None);

        var (isContiguous, hasOverlap) =
            await sut.CheckAsync((floor, ceiling) => floor + 1 == ceiling, CancellationToken.None);
        isContiguous.Should().BeFalse();
        hasOverlap.Should().BeTrue();

        var result = (await sut.FindAsync(15, CancellationToken.None)).ToArray();

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[]
        {
            (10, 15, "same start number"),
            (10, 19, "10-19")
        });
    }

    [InlineData(9)]
    [InlineData(10, 0, 1, 2, 3)]
    [InlineData(11, 0, 1, 2, 3)]
    [InlineData(12, 0, 1, 2, 3)]
    [InlineData(13, 0, 1, 2, 3)]
    [InlineData(14, 0, 1, 2, 3)]
    [InlineData(15, 0, 1, 2, 3)]
    [InlineData(16, 1, 2, 3)]
    [InlineData(17, 1, 2, 3)]
    [InlineData(18, 1, 2, 3)]
    [InlineData(19, 1, 2, 3)]
    [InlineData(20, 2, 3)]
    [InlineData(21, 2, 3)]
    [InlineData(22, 2, 3)]
    [InlineData(23, 3)]
    [InlineData(24, 3)]
    [InlineData(25, 3)]
    [InlineData(26)]
    [InlineData(49)]
    [InlineData(50, 4)]
    [InlineData(51, 4)]
    [InlineData(52, 4)]
    [InlineData(53)]
    [Theory]
    public async Task When_adding_multiple_values_that_overlap_then_they_should_still_work(int index,
        params int[] expectedIndexes)
    {
        var ranges = new (int floor, int ceiling, string value)[]
        {
            (10, 15, "10-15"),
            (10, 19, "10-19"),
            (10, 22, "10-22"),
            (10, 25, "10-25"),
            (50, 52, "50-52")
        };
        var sut = CreateSut();

        Task GoAddIt(IList<(int floor, int ceiling, string value)> r, IRangeSearcher<string, int> s, int i)
        {
            return s.AddAsync(r[i].floor, r[i].ceiling, r[i].value, CancellationToken.None);
        }

        await GoAddIt(ranges, sut, 4);
        await GoAddIt(ranges, sut, 1);
        await GoAddIt(ranges, sut, 3);
        await GoAddIt(ranges, sut, 0);
        await GoAddIt(ranges, sut, 2);

        var found = await sut.FindAsync(index, CancellationToken.None);
        var expected = expectedIndexes.Select(idx => ranges[idx]).ToArray();
        found.Should().BeEquivalentTo(expected);
    }
}