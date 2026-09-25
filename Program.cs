// Program.cs
using System.Reflection;
using GeoAuditEngineMc.Models;
using GeoAuditEngineMc.Rules;
using GeoAuditEngineMc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<AuditService>();

// Projedeki IAuditRule uygulayan TÜM sınıfları tek seferde otomatik kaydet
var ruleTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => typeof(IAuditRule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

foreach (var type in ruleTypes)
{
    builder.Services.AddTransient(typeof(IAuditRule), type);
}

var app = builder.Build();

app.MapPost("/api/audit", async (AuditRequest request, AuditService auditService) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest(new { error = "Geçersiz URL formatı." });
    }

    var results = await auditService.RunAuditAsync(request.Url);

    // Webtures formatında kategorilere ayırarak grupla
    var sections = results
        .GroupBy(r => new { r.SectionId, r.SectionTitle })
        .OrderBy(g => g.Key.SectionId)
        .Select(g => new
        {
            SectionId = g.Key.SectionId,
            SectionTitle = g.Key.SectionTitle,
            EarnedScore = g.Sum(r => r.EarnedScore),
            MaxScore = g.Sum(r => r.MaxScore),
            Percentage = g.Sum(r => r.MaxScore) > 0
                ? Math.Round((double)g.Sum(r => r.EarnedScore) / g.Sum(r => r.MaxScore) * 100, 0)
                : 0,
            Items = g.OrderBy(i => i.Id).ToList()
        });

    return Results.Ok(new
    {
        Url = request.Url,
        TotalEarned = results.Sum(r => r.EarnedScore),
        TotalMax = results.Sum(r => r.MaxScore),
        OverallPercentage = Math.Round((double)results.Sum(r => r.EarnedScore) / results.Sum(r => r.MaxScore) * 100, 0),
        Sections = sections
    });
});

app.Run();

record AuditRequest(string Url);