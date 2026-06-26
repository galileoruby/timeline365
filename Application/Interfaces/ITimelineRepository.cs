using TimeLine365.Application.DTOs;
using TimeLine365.Domain.Entities;

namespace TimeLine365.Application.Interfaces;

public interface ITimelineRepository
{
    Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(TimelineFilterRequest filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<int>> GetAvailableYearsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<int>> GetAvailableMonthsAsync(int year, CancellationToken cancellationToken);

    Task<IReadOnlyList<int>> GetAvailableDaysAsync(int year, int month, CancellationToken cancellationToken);
}
