namespace TimeLine365.Models;

public sealed class TimelineEventsIndexViewModel
{
    public string? StatusMessage { get; init; }
    public IReadOnlyList<TimelineEventListItemViewModel> Events { get; init; } = Array.Empty<TimelineEventListItemViewModel>();
}
