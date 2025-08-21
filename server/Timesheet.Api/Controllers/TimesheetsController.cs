using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Timesheet.Api.Data;
using Timesheet.Api.DTOs;
using Timesheet.Api.Models;
using System.Security.Claims;

namespace Timesheet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/timesheets")]
public class TimesheetsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TimesheetsController(AppDbContext db) => _db = db;

    Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Create
    [HttpPost]
    public async Task<ActionResult<TimesheetResponse>> Create([FromBody] CreateTimesheetRequest req, CancellationToken ct)
    {
        if (req is null) return BadRequest("Body is required.");
        if (req.LineItems is null || req.LineItems.Count == 0) return BadRequest("At least one line item is required.");
        if (req.LineItems.Any(li => li.Minutes < 0)) return BadRequest("Minutes must be >= 0.");
        if (req.Rate < 0) return BadRequest("Rate must be >= 0.");

        var userId = CurrentUserId();
        var entity = new TimesheetRecord
        {
            UserId = userId,
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
        var userId = CurrentUserId();
        var entity = await _db.Timesheets
            .Include(t => t.LineItems.OrderBy(li => li.Date))
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

        if (entity is null) return NotFound();
        return Ok(ToResponse(entity));
    }

    // List (scoped to current user)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetResponse>>> List(CancellationToken ct, int skip = 0, int take = 50)
    {
        var userId = CurrentUserId();

        var items = await _db.Timesheets
            .Where(t => t.UserId == userId)
            .Include(t => t.LineItems)
            .OrderByDescending(t => t.CreatedAt)
            .Skip(skip).Take(Math.Clamp(take, 1, 200))
            .ToListAsync(ct);

        return Ok(items.Select(ToResponse));
    }

    // Update
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> Update([FromRoute] Guid id, [FromBody] UpdateTimesheetRequest req, CancellationToken ct)
    {
        if (req is null) return BadRequest("Body is required.");
        if (req.LineItems is null) return BadRequest("Line items are required.");
        if (req.LineItems.Any(li => li.Minutes < 0)) return BadRequest("Minutes must be >= 0.");
        if (req.Rate < 0) return BadRequest("Rate must be >= 0.");

        var userId = CurrentUserId();

        var t = await _db.Timesheets
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (t is null) return NotFound();

        t.Description = req.Description;
        t.Rate = req.Rate;
        t.UpdatedAt = DateTime.UtcNow;

        // Bulk delete existing line items (avoids per-row concurrency issues)
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
            .FirstAsync(x => x.Id == id && x.UserId == userId, ct);

        return ToResponse(reloaded);
    }

    // Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId();

        var entity = await _db.Timesheets
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
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
