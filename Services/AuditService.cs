using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AngleSharp;
using GeoAuditEngineMc.Models;
using GeoAuditEngineMc.Rules;

namespace GeoAuditEngineMc.Services;

public class AuditService
{
    private readonly HttpClient _httpClient;
    private readonly IEnumerable<IAuditRule> _rules;

    public AuditService(HttpClient httpClient, IEnumerable<IAuditRule> rules)
    {
        _httpClient = httpClient;
        _rules = rules;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GeoAuditBot/1.0)");
    }

    public async Task<List<RuleResult>> RunAuditAsync(string url)
    {
        var uri = new Uri(url);
        var origin = uri.GetLeftPart(UriPartial.Authority);

        // 1. Dalga: Ana sayfa ve kritik ajan/protokol dosyalarını paralel çek
        var pageTask = FetchPageAsync(uri);
        var robotsTask = TryGetStringAsync($"{origin}/robots.txt");
        var llmsTask = TryGetStringAsync($"{origin}/llms.txt");
        var llmsFullTask = TryGetStringAsync($"{origin}/llms-full.txt");
        var agentsTask = TryGetStringAsync($"{origin}/AGENTS.md");
        var skillTask = TryGetStringAsync($"{origin}/skill.md");
        var openApiTask = TryGetStringAsync($"{origin}/openapi.json");

        await Task.WhenAll(
            pageTask,
            robotsTask,
            llmsTask,
            llmsFullTask,
            agentsTask,
            skillTask,
            openApiTask
        );

        var robotsTxt = await robotsTask;

        // 2. Dalga: Akıllı Sitemap tespiti (Önce robots.txt kontrolü, sonra fallbackler)
        var sitemapXml = await ResolveSitemapAsync(origin, robotsTxt);

        var (html, statusCode, responseTime) = await pageTask;

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(html));

        // @graph, dizi ve iç içe JSON-LD bloklarını düzleştirerek çıkar
        var jsonLdNodes = ExtractJsonLd(document);

        var auditContext = new AuditContext
        {
            TargetUri = uri,
            RawHtml = html,
            Document = document,
            RobotsTxt = robotsTxt,
            LlmsTxt = await llmsTask,
            LlmsFullTxt = await llmsFullTask,
            AgentsMd = await agentsTask,
            SkillMd = await skillTask,
            OpenApiJson = await openApiTask,
            SitemapXml = sitemapXml,
            JsonLdNodes = jsonLdNodes,
            ResponseStatusCode = statusCode,
            ResponseTimeMs = responseTime
        };

        var results = new List<RuleResult>();
        foreach (var rule in _rules.OrderBy(r => r.SectionId).ThenBy(r => r.Id))
        {
            var result = await rule.EvaluateAsync(auditContext);
            results.Add(result);
        }

        return results;
    }

    private async Task<string?> ResolveSitemapAsync(string origin, string? robotsTxt)
    {
        // 1. robots.txt içinde Sitemap: satırı tanımlanmış mı?
        if (!string.IsNullOrWhiteSpace(robotsTxt))
        {
            var match = Regex.Match(robotsTxt, @"Sitemap:\s*(https?://[^\s]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var customUrl = match.Groups[1].Value.Trim();
                var sitemap = await TryGetStringAsync(customUrl);
                if (IsValidSitemap(sitemap))
                    return sitemap;
            }
        }

        // 2. Standart /sitemap.xml ve /sitemap_index.xml yollarını paralel dene
        var sitemap1Task = TryGetStringAsync($"{origin}/sitemap.xml");
        var sitemap2Task = TryGetStringAsync($"{origin}/sitemap_index.xml");
        var sitemap3Task = TryGetStringAsync($"{origin}/wp-sitemap.xml");

        await Task.WhenAll(sitemap1Task, sitemap2Task, sitemap3Task);

        var s1 = await sitemap1Task;
        if (IsValidSitemap(s1)) return s1;

        var s2 = await sitemap2Task;
        if (IsValidSitemap(s2)) return s2;

        var s3 = await sitemap3Task;
        if (IsValidSitemap(s3)) return s3;

        return null;
    }

    private static bool IsValidSitemap(string? content)
    {
        return !string.IsNullOrWhiteSpace(content) &&
               (content.Contains("<urlset", StringComparison.OrdinalIgnoreCase) ||
                content.Contains("<sitemapindex", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<(string html, int statusCode, long responseTime)> FetchPageAsync(Uri uri)
    {
        var sw = Stopwatch.StartNew();
        var response = await _httpClient.GetAsync(uri);
        sw.Stop();
        var html = await response.Content.ReadAsStringAsync();
        return (html, (int)response.StatusCode, sw.ElapsedMilliseconds);
    }

    private async Task<string?> TryGetStringAsync(string endpoint)
    {
        try
        {
            var res = await _httpClient.GetAsync(endpoint);
            if (res.IsSuccessStatusCode)
                return await res.Content.ReadAsStringAsync();
        }
        catch { }
        return null;
    }

    private List<JsonNode> ExtractJsonLd(AngleSharp.Dom.IDocument document)
    {
        var nodes = new List<JsonNode>();
        var scriptElements = document.QuerySelectorAll("script[type='application/ld+json']");

        foreach (var element in scriptElements)
        {
            try
            {
                var content = element.InnerHtml.Trim();
                if (string.IsNullOrWhiteSpace(content)) continue;

                var parsed = JsonNode.Parse(content);
                if (parsed == null) continue;

                // 1. Durum: Kök nesne doğrudan bir dizi ise [ { ... }, { ... } ]
                if (parsed is JsonArray rootArray)
                {
                    foreach (var item in rootArray)
                    {
                        if (item != null) FlattenNode(item, nodes);
                    }
                }
                else
                {
                    FlattenNode(parsed, nodes);
                }
            }
            catch { }
        }
        return nodes;
    }

    private static void FlattenNode(JsonNode node, List<JsonNode> accumulator)
    {
        // @graph varsa altındaki tüm şemaları ayrı ayrı listeye ekle
        if (node["@graph"] is JsonArray graphArray)
        {
            foreach (var item in graphArray)
            {
                if (item != null) accumulator.Add(item);
            }
        }
        else
        {
            accumulator.Add(node);
        }
    }
}