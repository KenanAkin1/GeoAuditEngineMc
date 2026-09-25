// Models/AuditContext.cs
using System.Text.Json.Nodes;
using AngleSharp.Dom;

namespace GeoAuditEngineMc.Models;

public class AuditContext
{
    public Uri TargetUri { get; set; } = null!;
    public string RawHtml { get; set; } = string.Empty;
    public IDocument Document { get; set; } = null!;

    // Protokol ve Ajan Dosyaları
    public string? RobotsTxt { get; set; }
    public string? LlmsTxt { get; set; }
    public string? LlmsFullTxt { get; set; }
    public string? AgentsMd { get; set; }
    public string? SkillMd { get; set; }
    public string? OpenApiJson { get; set; }
    public string? SitemapXml { get; set; }

    // Yapısal Veriler
    public List<JsonNode> JsonLdNodes { get; set; } = new();

    // Ağ ve HTTP
    public int ResponseStatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
}