using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timesheet.Api.Models;

public class TimesheetRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Hourly rate in your currency (e.g., USD/hour).</summary>
    [Column(TypeName = "numeric(14,2)")]
    public decimal Rate { get; set; }

    public List<TimesheetLineItem> LineItems { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TimesheetLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TimesheetRecordId { get; set; }
    public TimesheetRecord? Timesheet { get; set; }

    /// <summary>Date the work happened.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Number of minutes worked for this line item.</summary>
    [Range(0, int.MaxValue)]
    public int Minutes { get; set; }

    /// <summary>Optional note for this line (not required by spec).</summary>
    public string? Notes { get; set; }
}
