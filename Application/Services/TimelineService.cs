using System.Globalization;
using TimeLine365.Application.DTOs;
using TimeLine365.Application.Interfaces;
using TimeLine365.Domain.Entities;

namespace TimeLine365.Application.Services;

public sealed class TimelineService : ITimelineService
{
    private readonly ITimelineRepository _repository;

    public TimelineService(ITimelineRepository repository)
    {
        _repository = repository;
    }

    public async Task<TimelinePageResponse> GetTimelineAsync(TimelineFilterRequest request, CancellationToken cancellationToken)
    {
        var normalized = NormalizeFilter(request);

        var yearsTask = _repository.GetAvailableYearsAsync(cancellationToken);
        var eventsTask = _repository.GetEventsAsync(normalized, cancellationToken);
        var monthsTask = normalized.Year.HasValue
            ? _repository.GetAvailableMonthsAsync(normalized.Year.Value, cancellationToken)
            : Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());
        var daysTask = normalized.Year.HasValue && normalized.Month.HasValue
            ? _repository.GetAvailableDaysAsync(normalized.Year.Value, normalized.Month.Value, cancellationToken)
            : Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());

        await Task.WhenAll(yearsTask, eventsTask, monthsTask, daysTask);

        var events = eventsTask.Result;
        var timeline = BuildTimeline(events);
        var options = new TimelineFilterOptionsResponse(yearsTask.Result, monthsTask.Result, daysTask.Result);

        return new TimelinePageResponse(normalized, options, timeline, events.Count);
    }

    private static TimelineFilterRequest NormalizeFilter(TimelineFilterRequest request)
    {
        int? year = request.Year;
        int? month = request.Month;
        int? day = request.Day;

        if (year is < 1 or > 9999)
        {
            year = null;
        }

        if (!year.HasValue)
        {
            return new TimelineFilterRequest(null, null, null);
        }

        if (month is < 1 or > 12)
        {
            month = null;
        }

        if (!month.HasValue)
        {
            return new TimelineFilterRequest(year, null, null);
        }

        if (day is < 1 or > 31)
        {
            day = null;
        }

        return new TimelineFilterRequest(year, month, day);
    }

    private static IReadOnlyList<TimelineYearGroupResponse> BuildTimeline(IReadOnlyList<TimelineEvent> events)
    {
        return events
            .GroupBy(x => x.Year)
            .Select(yearGroup => new TimelineYearGroupResponse(
                yearGroup.Key,
                yearGroup
                    .GroupBy(x => x.Month)
                    .Select(monthGroup => new TimelineMonthGroupResponse(
                        monthGroup.Key,
                        monthGroup.Key.HasValue
                            ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key.Value)
                            : "Year Overview",
                        monthGroup
                            .GroupBy(x => x.Day)
                            .Select(dayGroup => new TimelineDayGroupResponse(
                                dayGroup.Key,
                                dayGroup.Key.HasValue ? $"Day {dayGroup.Key.Value}" : "Month Overview",
                                dayGroup.Select(MapEvent).ToList()))
                            .ToList()))
                    .ToList()))
            .ToList();
    }

    private static TimelineEventResponse MapEvent(TimelineEvent item)
    {
        var dateLabel = item.Day.HasValue
            ? $"{item.Year:D4}-{item.Month:D2}-{item.Day:D2}"
            : item.Month.HasValue
                ? $"{item.Year:D4}-{item.Month:D2}"
                : $"{item.Year:D4}";

        return new TimelineEventResponse(
            item.Id,
            dateLabel,
            item.Title,
            item.Description,
            item.MediaUrl,
            item.ReferenceUrl);
    }
}
