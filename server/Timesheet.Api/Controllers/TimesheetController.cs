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

    [HttpPost]
    public async Task<ActionResult<TimesheetResponse>> Create(CreateTimesheetRequest req)
    {
        var t = new TimesheetRecord
        {
            Description = req.Description,
            Rate = req.Rate,
            LineItems = req.LineItems.Select(li => new TimesheetLineItem
            {
                Date = li.Date, Minutes = li.Minutes, Notes = li.Notes
            }).ToList(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Timesheets.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = t.Id }, ToResponse(t));
    }

    [HttpGet]
    public async Task<IEnumerable<TimesheetResponse>> List(int skip = 0, int take = 50)
    {
        var data = await _db.Timesheets
            .Include(x => x.LineItems)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync();

        return data.Select(ToResponse);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> GetById(Guid id)
    {
        var t = await _db.Timesheets
            .Include(x => x.LineItems.OrderBy(li => li.Date))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (t is null) return NotFound();
        return ToResponse(t);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TimesheetResponse>> Update(Guid id, UpdateTimesheetRequest req)
    {
        var t = await _db.Timesheets
            .Include(x => x.LineItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (t is null) return NotFound();

        t.Description = req.Description;
        t.Rate = req.Rate;

        _db.LineItems.RemoveRange(t.LineItems);
        t.LineItems = req.LineItems.Select(li => new TimesheetLineItem
        {
            Date = li.Date, Minutes = li.Minutes, Notes = li.Notes
        }).ToList();

        t.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToResponse(t);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var t = await _db.Timesheets.FindAsync(id);
        if (t is null) return NotFound();
        _db.Timesheets.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static TimesheetResponse ToResponse(TimesheetRecord t)
    {
        var totalMinutes = t.LineItems.Sum(li => li.Minutes);
        var totalHours = Math.Round((decimal)totalMinutes / 60m, 2);
        var totalCost  = Math.Round(totalHours * t.Rate, 2);

        return new TimesheetResponse(
            t.Id, t.Description, t.Rate,
            totalMinutes, totalHours, totalCost,
            t.LineItems.OrderBy(li => li.Date)
                       .Select(li => new LineItemView(li.Id, li.Date, li.Minutes, li.Notes))
                       .ToList(),
            t.CreatedAt, t.UpdatedAt
        );
    }
}
