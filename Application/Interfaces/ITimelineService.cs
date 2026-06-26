using TimeLine365.Application.DTOs;

namespace TimeLine365.Application.Interfaces;

public interface ITimelineService
{
    Task<TimelinePageResponse> GetTimelineAsync(TimelineFilterRequest request, CancellationToken cancellationToken);
}
