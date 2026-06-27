namespace TimeLine365.Models;

public sealed class TimelineEventDetailsViewModel
{
    public long Id { get; init; }
    public string DateLabel { get; init; } = string.Empty;
    public int Year { get; init; }
    public int? Month { get; init; }
    public int? Day { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? MediaUrl { get; init; }
    public string? ReferenceUrl { get; init; }
    public int SortOrder { get; init; }
}