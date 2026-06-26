namespace TimeLine365.Application.DTOs;

public sealed record TimelineYearGroupResponse(
    int Year,
    IReadOnlyList<TimelineMonthGroupResponse> Months);
