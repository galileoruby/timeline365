namespace TimeLine365.Application.DTOs;

public sealed record CreateTimelineEventRequest(
    int Year,
    int? Month,
    int? Day,
    string Title,
    string? Description,
    string? MediaUrl,
    string? ReferenceUrl,
    int SortOrder);
