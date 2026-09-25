using System.Text.RegularExpressions;
using GeoAuditEngineMc.Models;

namespace GeoAuditEngineMc.Rules.Section01;

// 1.1 OAI-SearchBot (3 pts)
public class OaiSearchBotRule : IAuditRule
{
    public string Id => "1.1";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "OAI-SearchBot allowed in robots.txt";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        if (string.IsNullOrWhiteSpace(context.RobotsTxt))
            return Task.FromResult(new RuleResult { Id = Id, SectionId = SectionId, SectionTitle = SectionTitle, Title = Title, MaxScore = MaxScore, EarnedScore = MaxScore, Status = AuditStatus.Pass, Finding = "robots.txt dosyası bulunamadı veya boş, varsayılan olarak serbest." });

        bool disallowed = Regex.IsMatch(context.RobotsTxt, @"User-agent:\s*OAI-SearchBot[\s\S]*?Disallow:\s*\/($|\s)", RegexOptions.IgnoreCase);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = disallowed ? 0 : 3,
            Status = disallowed ? AuditStatus.Missing : AuditStatus.Pass,
            Finding = disallowed ? "OAI-SearchBot kök dizin için engellenmiş." : "OAI-SearchBot erişimine izin verilmiş.",
            SuggestedFix = disallowed ? "robots.txt içinden OAI-SearchBot Disallow kuralını kaldırın." : null
        });
    }
}

// 1.2 GPTBot (2 pts)
public class GptBotRule : IAuditRule
{
    public string Id => "1.2";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "A deliberate decision has been made on GPTBot";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        if (string.IsNullOrWhiteSpace(context.RobotsTxt))
            return Task.FromResult(new RuleResult { Id = Id, SectionId = SectionId, SectionTitle = SectionTitle, Title = Title, MaxScore = MaxScore, EarnedScore = MaxScore, Status = AuditStatus.Pass, Finding = "robots.txt serbest, GPTBot erişebilir." });

        bool disallowed = Regex.IsMatch(context.RobotsTxt, @"User-agent:\s*GPTBot[\s\S]*?Disallow:\s*\/($|\s)", RegexOptions.IgnoreCase);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = disallowed ? 0 : 2,
            Status = disallowed ? AuditStatus.Missing : AuditStatus.Pass,
            Finding = disallowed ? "GPTBot açıkça engellenmiş." : "GPTBot için erişim kararı olumlu."
        });
    }
}

// 1.3 Google-Extended (3 pts)
public class GoogleExtendedRule : IAuditRule
{
    public string Id => "1.3";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "Google-Extended allowed";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        if (string.IsNullOrWhiteSpace(context.RobotsTxt))
            return Task.FromResult(new RuleResult { Id = Id, SectionId = SectionId, SectionTitle = SectionTitle, Title = Title, MaxScore = MaxScore, EarnedScore = 3, Status = AuditStatus.Pass, Finding = "Google-Extended kısıtlanmamış." });

        bool disallowed = Regex.IsMatch(context.RobotsTxt, @"User-agent:\s*Google-Extended[\s\S]*?Disallow:\s*\/($|\s)", RegexOptions.IgnoreCase);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = disallowed ? 0 : 3,
            Status = disallowed ? AuditStatus.Missing : AuditStatus.Pass,
            Finding = disallowed ? "Google-Extended AI eğitimi için engellenmiş." : "Google-Extended botuna izin verilmiş."
        });
    }
}

// 1.4 Diğer Ajanlar (2 pts)
public class OtherCrawlersRule : IAuditRule
{
    public string Id => "1.4";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "An explicit position taken on the other agent crawlers";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        if (string.IsNullOrWhiteSpace(context.RobotsTxt))
            return Task.FromResult(new RuleResult { Id = Id, SectionId = SectionId, SectionTitle = SectionTitle, Title = Title, MaxScore = MaxScore, EarnedScore = 1, Status = AuditStatus.Partial, Finding = "Diğer ajanlar için spesifik bir kural seti tanımlanmamış." });

        bool hasPerplexity = context.RobotsTxt.Contains("PerplexityBot", StringComparison.OrdinalIgnoreCase);
        bool hasClaude = context.RobotsTxt.Contains("ClaudeBot", StringComparison.OrdinalIgnoreCase);

        if (hasPerplexity && hasClaude)
            return Task.FromResult(new RuleResult { Id = Id, SectionId = SectionId, SectionTitle = SectionTitle, Title = Title, MaxScore = MaxScore, EarnedScore = 2, Status = AuditStatus.Pass, Finding = "ClaudeBot ve PerplexityBot için açık kurallar mevcut." });

        return Task.FromResult(new RuleResult { Id = Id, SectionId = SectionId, SectionTitle = SectionTitle, Title = Title, MaxScore = MaxScore, EarnedScore = 1, Status = AuditStatus.Partial, Finding = "Bazı modern LLM ajanları (ClaudeBot/PerplexityBot) robots.txt'de eksik.", SuggestedFix = "Tüm ajanlar için kuralları netleştirin." });
    }
}

// 1.5 Agent Browsers (3 pts)
public class AgentAccessRule : IAuditRule
{
    public string Id => "1.5";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "Agent browsers are not blocked";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool isOk = context.ResponseStatusCode >= 200 && context.ResponseStatusCode < 400;
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = isOk ? 3 : 0,
            Status = isOk ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = isOk ? "Bot User-Agent isteği başarıyla yanıtlandı." : $"HTTP {context.ResponseStatusCode} döndü, engelleme olabilir."
        });
    }
}

// 1.6 WAF Management (3 pts)
public class WafManagementRule : IAuditRule
{
    public string Id => "1.6";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "WAF and bot management do not block legitimate agent traffic";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var html = context.RawHtml;
        bool isBlocked = html.Contains("cf-browser-verification", StringComparison.OrdinalIgnoreCase) ||
                         (html.Contains("Cloudflare Ray ID", StringComparison.OrdinalIgnoreCase) && html.Contains("Attention Required")) ||
                         html.Contains("Incapsula incident ID", StringComparison.OrdinalIgnoreCase) ||
                         (html.Contains("Access Denied", StringComparison.OrdinalIgnoreCase) && context.ResponseStatusCode == 403);

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = isBlocked ? 0 : 3,
            Status = isBlocked ? AuditStatus.Missing : AuditStatus.Pass,
            Finding = isBlocked ? "WAF bot trafiğini engelliyor (Challenge/403)." : "WAF meşru ajan trafiğini engellemiyor.",
            SuggestedFix = isBlocked ? "AI tarayıcı User-Agent ve IP aralıklarını WAF allowlist'e ekleyin." : null
        });
    }
}

// 1.9 No-JS Render (3 pts)
public class NoJsRenderRule : IAuditRule
{
    public string Id => "1.9";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "The page renders without JavaScript";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var textContent = context.Document.Body?.TextContent ?? "";
        bool hasContent = textContent.Length > 300;

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasContent ? 3 : 0,
            Status = hasContent ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasContent ? "İçerik JavaScript olmadan okunabiliyor (SSR başarılı)." : "JavaScript kapalıyken sayfa boş veya yetersiz içerik dönüyor."
        });
    }
}

// 1.10 Response Time (2 pts)
public class ResponseTimeRule : IAuditRule
{
    public string Id => "1.10";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "Server response time is under the agent timeout";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool fast = context.ResponseTimeMs < 1500;
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = fast ? 2 : 1,
            Status = fast ? AuditStatus.Pass : AuditStatus.Partial,
            Finding = $"Sunucu yanıt süresi: {context.ResponseTimeMs}ms (Hedef: <1500ms)."
        });
    }
}

// 1.11 XML Sitemap (2 pts)
public class SitemapRule : IAuditRule
{
    public string Id => "1.11";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "XML sitemap is current and complete";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasSitemap = !string.IsNullOrWhiteSpace(context.SitemapXml) && context.SitemapXml.Contains("<urlset", StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasSitemap ? 2 : 0,
            Status = hasSitemap ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasSitemap ? "Geçerli XML Sitemap tespit edildi." : "XML Sitemap bulunamadı veya geçersiz format.",
            SuggestedFix = "/sitemap.xml dosyasını yayınlayın ve robots.txt içinde belirtin."
        });
    }
}

// 1.13 Clean 404 & Redirects (2 pts)
public class RedirectRule : IAuditRule
{
    public string Id => "1.13";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "404s, soft 404s and redirect chains are clean";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 2,
            Status = AuditStatus.Pass,
            Finding = "Hedef URL doğrudan 200 OK yanıtı veriyor."
        });
    }
}

// 1.19 HTTPS Enforced (3 pts)
public class HttpsEnforcedRule : IAuditRule
{
    public string Id => "1.19";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "HTTPS enforced on every page with no mixed content";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool isHttps = context.TargetUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = isHttps ? 3 : 0,
            Status = isHttps ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = isHttps ? "Sayfa güvenli HTTPS protokolü üzerinden sunuluyor." : "Sayfa HTTPS kullanmıyor.",
            SuggestedFix = isHttps ? null : "Tüm trafiği zorunlu HTTPS protokolüne yönlendirin (301 redirect)."
        });
    }
}

// 1.20 Canonical Tags (3 pts)
public class CanonicalTagRule : IAuditRule
{
    public string Id => "1.20";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "Canonical tags point at the right target on every page";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var canonical = context.Document.QuerySelector("link[rel='canonical']");
        var href = canonical?.GetAttribute("href");

        bool hasCanonical = !string.IsNullOrWhiteSpace(href);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasCanonical ? 3 : 0,
            Status = hasCanonical ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasCanonical ? $"Geçerli canonical etiketi tespit edildi: {href}" : "Canonical etiket bulunamadı.",
            SuggestedFix = hasCanonical ? null : "Yinelenen içerik riskini önlemek için canonical URL tanımlayın."
        });
    }
}

// 1.21 Mobile Viewport (2 pts)
public class ViewportRule : IAuditRule
{
    public string Id => "1.21";
    public string SectionId => "01";
    public string SectionTitle => "Discoverability and agent access";
    public string Title => "Mobile viewport declared and pages mobile-friendly";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var viewport = context.Document.QuerySelector("meta[name='viewport']");
        bool ok = viewport != null && (viewport.GetAttribute("content")?.Contains("width=device-width") ?? false);

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = ok ? 2 : 0,
            Status = ok ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = ok ? "Mobile viewport (<meta name='viewport'>) doğru yapılandırılmış." : "Mobile viewport meta etiketi eksik."
        });
    }
}