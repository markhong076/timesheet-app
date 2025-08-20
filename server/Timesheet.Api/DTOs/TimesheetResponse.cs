namespace Timesheet.Api.DTOs;

public record TimesheetResponse(
    Guid Id,
    string? Description,
    decimal Rate,
    int TotalMinutes,
    decimal TotalHours,
    decimal TotalCost,
    List<LineItemView> LineItems,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record LineItemView(Guid Id, DateOnly Date, int Minutes, string? Notes);
