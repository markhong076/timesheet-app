using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timesheet.Api.Data;
using Timesheet.Api.DTOs;
using Timesheet.Api.Models;

namespace Timesheet.Api.Controllers;

[ApiController]
[Route("api/timesheets")]
public class TimesheetsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TimesheetsController(AppDbContext db) => _db = db;

    // Create
    [HttpPost]
    public async Task<ActionResult<TimesheetResponse>> Create([FromBody] CreateTimesheetRequest req, CancellationToken ct)
    {
        if (req.LineItems.Any(li => li.Minutes < 0))
            return BadRequest("Minutes must be >= 0.");

        var entity = new TimesheetRecord
        {
            Description = req.Description,
            Rate = req.Rate,
            LineItems = req.LineItems.Select(li => new TimesheetLineItem
            {
                Date = li.Date,
                Minutes = li.Minutes,
                Notes = li.Notes
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Timesheets.Add(entity);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToResponse(entity));
    }

    // Read one
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var entity = await _db.Timesheets
            .Include(t => t.LineItems.OrderBy(li => li.Date))
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (entity is null) return NotFound();
        return Ok(ToResponse(entity));
    }

    // List (simple)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetResponse>>> List(CancellationToken ct, int skip = 0, int take = 50)
    {
        var items = await _db.Timesheets
            .Include(t => t.LineItems)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

        return Ok(items.Select(ToResponse));
    }

    // Update
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> Update([FromRoute] Guid id, [FromBody] UpdateTimesheetRequest req, CancellationToken ct)
    {
        var t = await _db.Timesheets.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return NotFound();

        t.Description = req.Description;
        t.Rate = req.Rate;
        t.UpdatedAt = DateTime.UtcNow;

        await _db.LineItems
            .Where(li => li.TimesheetRecordId == id)
            .ExecuteDeleteAsync(ct);

        var newItems = req.LineItems.Select(li => new TimesheetLineItem
        {
            Id = Guid.NewGuid(),
            TimesheetRecordId = id,
            Date = li.Date,
            Minutes = li.Minutes,
            Notes = li.Notes
        }).ToList();

        await _db.LineItems.AddRangeAsync(newItems, ct);

        await _db.SaveChangesAsync(ct);

        var reloaded = await _db.Timesheets
            .Include(x => x.LineItems)
            .FirstAsync(x => x.Id == id, ct);

        return ToResponse(reloaded);
    }

    // Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var entity = await _db.Timesheets.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null) return NotFound();
        _db.Timesheets.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static TimesheetResponse ToResponse(TimesheetRecord t)
    {
        var totalMinutes = t.LineItems.Sum(li => li.Minutes);
        var totalHours = Math.Round((decimal)totalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
        var totalCost = Math.Round(totalHours * t.Rate, 2, MidpointRounding.AwayFromZero);

        return new TimesheetResponse(
            t.Id,
            t.Description,
            t.Rate,
            totalMinutes,
            totalHours,
            totalCost,
            t.LineItems
                .OrderBy(li => li.Date)
                .Select(li => new LineItemView(li.Id, li.Date, li.Minutes, li.Notes))
                .ToList(),
            t.CreatedAt,
            t.UpdatedAt
        );
    }
}
