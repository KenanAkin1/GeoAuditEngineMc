using GeoAuditEngineMc.Models;

namespace GeoAuditEngineMc.Rules;

public interface IAuditRule
{
    string Id { get; }
    string SectionId { get; }
    string SectionTitle { get; }
    string Title { get; }
    int MaxScore { get; }
    Task<RuleResult> EvaluateAsync(AuditContext context);
}