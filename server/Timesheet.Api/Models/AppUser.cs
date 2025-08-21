using System.ComponentModel.DataAnnotations;

namespace Timesheet.Api.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(320)]
    public string Email { get; set; } = default!;

    [Required, MaxLength(200)]
    public string PasswordHash { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TimesheetRecord> Timesheets { get; set; } = new();
}
