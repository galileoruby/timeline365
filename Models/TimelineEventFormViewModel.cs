using System.ComponentModel.DataAnnotations;

namespace TimeLine365.Models;

public sealed class TimelineEventFormViewModel : IValidatableObject
{
    public long Id { get; set; }

    [Required]
    [Range(1, 9999)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int? Month { get; set; }

    [Range(1, 31)]
    public int? Day { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Display(Name = "Media URL")]
    [Url]
    public string? MediaUrl { get; set; }

    [Display(Name = "Reference URL")]
    [Url]
    public string? ReferenceUrl { get; set; }

    [Display(Name = "Sort order")]
    public int SortOrder { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Day.HasValue && !Month.HasValue)
        {
            yield return new ValidationResult(
                "Month is required when day is provided.",
                new[] { nameof(Month), nameof(Day) });
        }
    }
}