namespace TimeLine365.Application.DTOs;

public sealed record TimelineEventResponse(
    long Id,
    string DateLabel,
    string Title,
    string? Description,
    string? MediaUrl,
    string? ReferenceUrl);
