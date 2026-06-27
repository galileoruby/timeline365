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

    public Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public Task<TimelineEvent?> GetEventAsync(long id, CancellationToken cancellationToken)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<long> CreateEventAsync(CreateTimelineEventRequest request, CancellationToken cancellationToken)
    {
        var validated = ValidateEvent(
            0,
            request.Year,
            request.Month,
            request.Day,
            request.Title,
            request.Description,
            request.MediaUrl,
            request.ReferenceUrl,
            request.SortOrder);

        var normalizedRequest = new CreateTimelineEventRequest(
            validated.Year,
            validated.Month,
            validated.Day,
            validated.Title,
            validated.Description,
            validated.MediaUrl,
            validated.ReferenceUrl,
            validated.SortOrder);

        return await _repository.CreateAsync(normalizedRequest, cancellationToken);
    }

    public async Task<bool> UpdateEventAsync(UpdateTimelineEventRequest request, CancellationToken cancellationToken)
    {
        var validated = ValidateEvent(
            request.Id,
            request.Year,
            request.Month,
            request.Day,
            request.Title,
            request.Description,
            request.MediaUrl,
            request.ReferenceUrl,   
            request.SortOrder);

        var normalizedRequest = new UpdateTimelineEventRequest(
            validated.Id,
            validated.Year,
            validated.Month,
            validated.Day,
            validated.Title,
            validated.Description,
            validated.MediaUrl,
            validated.ReferenceUrl,
            validated.SortOrder);

        return await _repository.UpdateAsync(normalizedRequest, cancellationToken);
    }

    public Task<bool> DeleteEventAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Task.FromResult(false);
        }
        return _repository.DeleteAsync(id, cancellationToken);
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

    private static TimelineEvent ValidateEvent(
        long id,
        int year,
        int? month,
        int? day,
        string title,
        string? description,
        string? mediaUrl,
        string? referenceUrl,
        int sortOrder)
    {
        return new TimelineEvent(
            id,
            year,
            month,
            day,
            title,
            description,
            mediaUrl,
            referenceUrl,
            sortOrder);
    }

    private static IReadOnlyList<TimelineYearGroupResponse> BuildTimeline(IReadOnlyList<TimelineEvent> events)
    {
        return events
            .GroupBy(x => x.Year)
            .OrderByDescending(x => x.Key)
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