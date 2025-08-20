using System.ComponentModel.DataAnnotations;

namespace Timesheet.Api.DTOs;

public record LineItemDto(DateOnly Date, int Minutes, string? Notes);

public class CreateTimesheetRequest
{
    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Rate { get; set; }

    [MinLength(1)]
    public List<LineItemDto> LineItems { get; set; } = new();
}

public class UpdateTimesheetRequest : CreateTimesheetRequest {}
