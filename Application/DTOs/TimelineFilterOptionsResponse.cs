namespace TimeLine365.Application.DTOs;

public sealed record TimelineFilterOptionsResponse(
    IReadOnlyList<int> Years,
    IReadOnlyList<int> Months,
    IReadOnlyList<int> Days);
