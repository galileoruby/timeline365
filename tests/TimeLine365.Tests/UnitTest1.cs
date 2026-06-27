using TimeLine365.Application.DTOs;
using TimeLine365.Application.DTOs;
using TimeLine365.Application.Interfaces;
using TimeLine365.Application.Services;
using TimeLine365.Domain.Entities;

namespace TimeLine365.Tests;

public sealed class TimelineEventTests
{
    [Fact]
    public void Constructor_Should_Throw_When_DayWithoutMonth()
    {
        Assert.Throws<ArgumentException>(() =>
            new TimelineEvent(1, 2025, null, 10, "Event", null, null, null, 0));
    }

    [Fact]
    public void Constructor_Should_Trim_TextFields()
    {
        var item = new TimelineEvent(7, 2025, 6, 1, "  Launch  ", "  Detail  ", "  https://media  ", "  https://ref  ", 1);

        Assert.Equal("Launch", item.Title);
        Assert.Equal("Detail", item.Description);
        Assert.Equal("https://media", item.MediaUrl);
        Assert.Equal("https://ref", item.ReferenceUrl);
    }
}

public sealed class TimelineServiceTests
{
    [Fact]
    public async Task GetTimelineAsync_Should_Normalize_Invalid_Filter_To_Nulls()
    {
        var repo = new FakeTimelineRepository
        {
            Events =
            [
                new TimelineEvent(1, 2024, 6, 1, "Kickoff", "Initial event", null, null, 1)
            ],
            Years = [2024],
            Months = [6],
            Days = [1]
        };

        var sut = new TimelineService(repo);

        var result = await sut.GetTimelineAsync(new TimelineFilterRequest(0, 15, 99), CancellationToken.None);

        Assert.Null(result.SelectedFilter.Year);
        Assert.Null(result.SelectedFilter.Month);
        Assert.Null(result.SelectedFilter.Day);
        Assert.Equal(1, result.TotalEvents);
        Assert.Single(result.Timeline);
    }

    [Fact]
    public async Task GetTimelineAsync_Should_Build_Hierarchical_Groups()
    {
        var repo = new FakeTimelineRepository
        {
            Events =
            [
                new TimelineEvent(1, 2024, 6, 1, "Vision", "v1", null, null, 1),
                new TimelineEvent(2, 2024, 6, 19, "Freeze", "v2", null, null, 2),
                new TimelineEvent(3, 2024, 7, null, "Month milestone", null, null, null, 1),
                new TimelineEvent(4, 2025, null, null, "Year overview", null, null, null, 1)
            ],
            Years = [2024, 2025],
            Months = [6, 7],
            Days = [1, 19]
        };

        var sut = new TimelineService(repo);

        var result = await sut.GetTimelineAsync(new TimelineFilterRequest(2024, 6, null), CancellationToken.None);

        Assert.Equal(new TimelineFilterRequest(2024, 6, null), result.SelectedFilter);
        Assert.Equal(4, result.TotalEvents);
        Assert.Equal(2, result.Timeline.Count);

        var year2024 = result.Timeline.Single(x => x.Year == 2024);
        Assert.Equal(2, year2024.Months.Count);

        var june = year2024.Months.Single(x => x.Month == 6);
        Assert.Equal(6, june.Month);
        Assert.Equal(2, june.Days.Count);

        var dayOne = june.Days.Single(x => x.Day == 1);
        Assert.Equal("2024-06-01", dayOne.Events.Single().DateLabel);
    }

    private sealed class FakeTimelineRepository : ITimelineRepository
    {
        public IReadOnlyList<TimelineEvent> Events { get; init; } = [];

        public IReadOnlyList<int> Years { get; init; } = [];

        public IReadOnlyList<int> Months { get; init; } = [];

        public IReadOnlyList<int> Days { get; init; } = [];

        public long CreatedId { get; init; } = 999;

        public Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(TimelineFilterRequest filter, CancellationToken cancellationToken)
            => Task.FromResult(Events);

        public Task<IReadOnlyList<TimelineEvent>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult(Events);

        public Task<TimelineEvent?> GetByIdAsync(long id, CancellationToken cancellationToken)
            => Task.FromResult(Events.SingleOrDefault(x => x.Id == id));

        public Task<long> CreateAsync(CreateTimelineEventRequest request, CancellationToken cancellationToken)
            => Task.FromResult(CreatedId);

        public Task<bool> UpdateAsync(UpdateTimelineEventRequest request, CancellationToken cancellationToken)
            => Task.FromResult(Events.Any(x => x.Id == request.Id));

        public Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
            => Task.FromResult(Events.Any(x => x.Id == id));

        public Task<IReadOnlyList<int>> GetAvailableYearsAsync(CancellationToken cancellationToken)
            => Task.FromResult(Years);

        public Task<IReadOnlyList<int>> GetAvailableMonthsAsync(int year, CancellationToken cancellationToken)
            => Task.FromResult(Months);

        public Task<IReadOnlyList<int>> GetAvailableDaysAsync(int year, int month, CancellationToken cancellationToken)
            => Task.FromResult(Days);
    }
}
