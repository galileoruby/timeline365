namespace TimeLine365.Application.DTOs;

public sealed record TimelinePageResponse(
    TimelineFilterRequest SelectedFilter,
    TimelineFilterOptionsResponse FilterOptions,
    IReadOnlyList<TimelineYearGroupResponse> Timeline,
    int TotalEvents);
