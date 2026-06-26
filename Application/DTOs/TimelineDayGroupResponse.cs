namespace TimeLine365.Application.DTOs;

public sealed record TimelineDayGroupResponse(
    int? Day,
    string Label,
    IReadOnlyList<TimelineEventResponse> Events);
