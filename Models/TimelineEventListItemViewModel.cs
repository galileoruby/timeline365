namespace TimeLine365.Models;

public sealed class TimelineEventListItemViewModel
{
    public long Id { get; init; }
    public string DateLabel { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}