using TimeLine365.Application.DTOs;
using TimeLine365.Domain.Entities;
namespace TimeLine365.Application.Interfaces;

public interface ITimelineService
{
    Task<TimelinePageResponse> GetTimelineAsync(TimelineFilterRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(CancellationToken cancellationToken);
    Task<TimelineEvent?> GetEventAsync(long id, CancellationToken cancellationToken);
    Task<long> CreateEventAsync(CreateTimelineEventRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateEventAsync(UpdateTimelineEventRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteEventAsync(long id, CancellationToken cancellationToken);
}