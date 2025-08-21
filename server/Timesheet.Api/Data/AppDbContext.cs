using Microsoft.EntityFrameworkCore;
using Timesheet.Api.Models;

namespace Timesheet.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TimesheetRecord> Timesheets => Set<TimesheetRecord>();
    public DbSet<TimesheetLineItem> LineItems => Set<TimesheetLineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(320);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<TimesheetRecord>(e =>
        {
            e.ToTable("timesheets");
            e.HasKey(t => t.Id);
            e.Property(t => t.Description).HasMaxLength(2000);
            e.Property(t => t.Rate).HasPrecision(14, 2);
            e.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasOne(t => t.User)
             .WithMany(u => u.Timesheets)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(t => t.LineItems)
             .WithOne(li => li.Timesheet)
             .HasForeignKey(li => li.TimesheetRecordId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimesheetLineItem>(e =>
        {
            e.ToTable("timesheet_line_items");
            e.HasKey(li => li.Id);
            e.Property(li => li.Minutes).IsRequired();
            e.Property(li => li.Notes).HasMaxLength(1000);
            // Map DateOnly to date
            e.Property(li => li.Date).HasColumnType("date");
        });
    }
}
