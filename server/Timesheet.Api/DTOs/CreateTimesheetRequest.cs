using System.ComponentModel.DataAnnotations;

namespace Timesheet.Api.DTOs;

public record LineItemDto(
    DateOnly Date,
    int Minutes,
    string? Notes
);

public class CreateTimesheetRequest
{
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Hourly rate (e.g., 120.50 means $120.50/hour).</summary>
    [Range(0, double.MaxValue)]
    public decimal Rate { get; set; }

    [MinLength(1)]
    public List<LineItemDto> LineItems { get; set; } = new();
}

public class UpdateTimesheetRequest
{
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Rate { get; set; }

    [MinLength(1)]
    public List<LineItemDto> LineItems { get; set; } = new();
}
