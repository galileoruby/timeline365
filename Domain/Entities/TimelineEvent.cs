namespace TimeLine365.Domain.Entities;

public sealed class TimelineEvent
{
    public TimelineEvent(
        long id,
        int year,
        int? month,
        int? day,
        string title,
        string? description,
        string? mediaUrl,
        string? referenceUrl,
        int sortOrder)
    {
        if (year < 1 || year > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12 when provided.");
        }

        if (day.HasValue && !month.HasValue)
        {
            throw new ArgumentException("Day cannot be provided without month.", nameof(day));
        }

        if (day is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31 when provided.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Id = id;
        Year = year;
        Month = month;
        Day = day;
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        MediaUrl = string.IsNullOrWhiteSpace(mediaUrl) ? null : mediaUrl.Trim();
        ReferenceUrl = string.IsNullOrWhiteSpace(referenceUrl) ? null : referenceUrl.Trim();
        SortOrder = sortOrder;
    }

    public long Id { get; }

    public int Year { get; }

    public int? Month { get; }

    public int? Day { get; }

    public string Title { get; }

    public string? Description { get; }

    public string? MediaUrl { get; }

    public string? ReferenceUrl { get; }

    public int SortOrder { get; }
}
