namespace GeoAuditEngineMc.Models;

public class RuleResult
{
    public string Id { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public int EarnedScore { get; set; }
    public AuditStatus Status { get; set; }
    public string Finding { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
}