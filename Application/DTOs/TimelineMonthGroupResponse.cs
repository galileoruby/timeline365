namespace TimeLine365.Application.DTOs;

public sealed record TimelineMonthGroupResponse(
    int? Month,
    string Label,
    IReadOnlyList<TimelineDayGroupResponse> Days);
