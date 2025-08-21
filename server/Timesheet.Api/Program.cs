using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Timesheet.Api.Data;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ---- Connection string ----
string? cs =
    config.GetConnectionString("Default") ??
    config.GetConnectionString("DefaultConnection") ??
    config["ConnectionStrings:Default"] ??
    config["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("No Postgres connection string found. Set ConnectionStrings:Default (or DefaultConnection).");

// ---- Services ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs));

// CORS (dev + prod via env)
var allowedOrigins = (config["AllowedOrigins"] ?? "")
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var allowAllOrigins = string.Equals(config["AllowAllOrigins"], "true", StringComparison.OrdinalIgnoreCase);

builder.Services.AddCors(o =>
{
    o.AddPolicy("DevCors", p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());

    o.AddPolicy("ProdCors", p =>
    {
        if (allowAllOrigins || allowedOrigins.Length == 0)
        {
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
    });
});

// JWT auth
var jwt = config.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---- Forwarded headers (Azure front door / ACA) ----
var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
};
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

// (Optional) keep this — with forwarded headers, redirects won’t break preflight
app.UseHttpsRedirection();

// CORS MUST be before auth so preflight gets headers
var corsPolicyName = app.Environment.IsDevelopment() ? "DevCors" : "ProdCors";
app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

// ---- Apply migrations on startup ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Swagger (enable in prod with ENABLE_SWAGGER=true)
var enableSwagger = app.Environment.IsDevelopment()
    || string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Simple checks
app.MapGet("/", () => Results.Text("Timesheet API is running", "text/plain"));
app.MapGet("/healthz", () => Results.Ok(new { ok = true, time = DateTime.UtcNow }));
app.MapGet("/readyz", async (AppDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok(new { db = "ok" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.Run();
