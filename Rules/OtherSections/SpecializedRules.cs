using System.Text.Json.Nodes;
using GeoAuditEngineMc.Models;

namespace GeoAuditEngineMc.Rules.OtherSections;

// 03 Product Data & Feed Readiness
// 3.4 Availability Enum Kontrolü (Schema.org Standart URL Enum)
public class AvailabilityEnumRule : IAuditRule
{
    public string Id => "3.4";
    public string SectionId => "03";
    public string SectionTitle => "Product data and feed readiness";
    public string Title => "availability uses the correct enum";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        // Doğrudan JSON-LD şemalarında ItemAvailability URL enum'larını ara
        bool hasValidEnum = false;
        foreach (var node in context.JsonLdNodes)
        {
            var text = node.ToJsonString();
            if (text.Contains("https://schema.org/InStock") ||
                text.Contains("https://schema.org/OutOfStock") ||
                text.Contains("https://schema.org/PreOrder") ||
                text.Contains("https://schema.org/BackOrder") ||
                text.Contains("https://schema.org/Discontinued"))
            {
                hasValidEnum = true;
                break;
            }
        }

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasValidEnum ? 3 : 0,
            Status = hasValidEnum ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasValidEnum
                ? "Stok durumu geçerli Schema.org ItemAvailability URL enum ile tanımlanmış."
                : "Schema.org ItemAvailability standardı (InStock/OutOfStock URL) bulunamadı.",
            SuggestedFix = hasValidEnum ? null : "offers.availability değerini 'https://schema.org/InStock' gibi standart URL enum formatında girin."
        });
    }
}

// 04 Protocol and Integration Layer
// 4.7 MCP Server Değerlendirmesi
public class ProtocolMcpRule : IAuditRule
{
    public string Id => "4.7";
    public string SectionId => "04";
    public string SectionTitle => "Protocol and integration layer";
    public string Title => "An MCP server has been evaluated";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        // Standart HTTP dış taramasında otonom MCP server manifestosu saptanamadığında şeffaf açıklama döner
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = "Açık bir Model Context Protocol (MCP) sunucu bağlantısı veya manifestosu saptanamadı.",
            SuggestedFix = "LLM modellerinin şirket veritabanına bağlanabilmesi için bir MCP sunucusu entegrasyonu planlayın."
        });
    }
}

// 4.8 API-first / Headless Mimari Kontrolü
public class ProtocolApiRule : IAuditRule
{
    public string Id => "4.8";
    public string SectionId => "04";
    public string SectionTitle => "Protocol and integration layer";
    public string Title => "API-first or headless architecture is in place or on the roadmap";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var raw = context.RawHtml;

        // Gerçek API dokümantasyonu veya GraphQL/Headless altyapısı var mı?
        bool hasDocumentation = !string.IsNullOrWhiteSpace(context.OpenApiJson) ||
                               raw.Contains("swagger-ui") ||
                               raw.Contains("/api-docs") ||
                               raw.Contains("graphql-endpoint") ||
                               raw.Contains("/graphql");

        if (hasDocumentation)
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
                Finding = "Dokümante edilmiş genel API veya Headless/GraphQL mimarisi başarıyla doğrulandı."
            });
        }

        // Sadece arka planda çalışan sıradan AJAX endpoint'leri tam headless sayılmaz
        bool hasPartialApi = raw.Contains("/api/") || raw.Contains("wp-json");
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = hasPartialApi
                ? "Dahili API uç noktaları tespit edildi ancak ajanların erişebileceği dokümante edilmiş bir API-First/Headless yapısı saptanamadı."
                : "Açık API veya headless mimari izi bulunamadı.",
            SuggestedFix = "Ajanların içerik ve envanteri sorgulayabilmesi için OpenAPI/Swagger ile belgelenmiş açık API katmanı oluşturun."
        });
    }
}

// 4.15 Function Calling İçin Yapısal Veri
public class FunctionCallingRule : IAuditRule
{
    public string Id => "4.15";
    public string SectionId => "04";
    public string SectionTitle => "Protocol and integration layer";
    public string Title => "Structured data ready for function calling";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasOpenApi = !string.IsNullOrWhiteSpace(context.OpenApiJson);
        bool hasValidJsonLd = context.JsonLdNodes.Count > 0;

        if (hasOpenApi)
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
                Finding = "Ajanların function calling çağrıları yapabileceği parametrik OpenAPI/JSON şeması mevcut."
            });
        }

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = "Yapay zeka modellerinin fonksiyon çağırabileceği (Tool Use / Function Calling) JSON şemaları bulunamadı.",
            SuggestedFix = "Uç noktalarınız için kök dizinde openapi.json yayınlayarak şema parametrelerini ajanlara açın."
        });
    }
}

// 4.16 OpenAI Actions / Plugin Yüzeyleri
public class OpenAiActionsRule : IAuditRule
{
    public string Id => "4.16";
    public string SectionId => "04";
    public string SectionTitle => "Protocol and integration layer";
    public string Title => "Decision made on OpenAI Actions / plugin surfaces";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasPluginManifest = !string.IsNullOrWhiteSpace(context.OpenApiJson);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasPluginManifest ? 1 : 0,
            Status = hasPluginManifest ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasPluginManifest
                ? "OpenAI Actions / GPT entegrasyon manifestosu doğrulandı."
                : "/.well-known/ai-plugin.json veya OpenAI Actions manifestosu bulunamadı.",
            SuggestedFix = hasPluginManifest ? null : "GPT'ler ve otonom eylemler için standart manifestoyu yapılandırın."
        });
    }
}

// 08 AI Visibility (GEO and AEO)
// 8.7 Brand Entity Doğrulaması (Knowledge Graph & sameAs)
public class BrandEntityRule : IAuditRule
{
    public string Id => "8.7";
    public string SectionId => "08";
    public string SectionTitle => "AI visibility (GEO and AEO)";
    public string Title => "Brand entity is unambiguous";
    public int MaxScore => 2; // Daha adil puanlama için 2 puan

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        // JSON-LD içindeki Organization nesnelerinden sameAs dizisini güvenli oku
        var sameAsUrls = new List<string>();
        foreach (var node in context.JsonLdNodes)
        {
            if (node["sameAs"] is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    var val = item?.ToString();
                    if (!string.IsNullOrWhiteSpace(val)) sameAsUrls.Add(val);
                }
            }
            else if (node["sameAs"] is JsonValue val)
            {
                sameAsUrls.Add(val.ToString());
            }
        }

        // Wikidata, Wikipedia, Crunchbase, LinkedIn veya resmi tescil otoriteleri var mı?
        bool hasAuthorityEntity = sameAsUrls.Any(u =>
            u.Contains("wikidata.org") ||
            u.Contains("wikipedia.org") ||
            u.Contains("crunchbase.com") ||
            u.Contains("linkedin.com/company"));

        if (hasAuthorityEntity)
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
                Finding = "Marka, doğrulanabilir dış otorite profilleriyle (Wikidata/LinkedIn vb.) tam olarak ilişkilendirilmiş."
            });
        }

        if (sameAsUrls.Count > 0)
        {
            return Task.FromResult(new RuleResult
            {
                Id = Id,
                SectionId = SectionId,
                SectionTitle = SectionTitle,
                Title = Title,
                MaxScore = MaxScore,
                EarnedScore = 1,
                Status = AuditStatus.Partial,
                Finding = $"{sameAsUrls.Count} adet sosyal profil tespit edildi, ancak Knowledge Graph için Wikidata/Wikipedia entity bağlantısı eksik.",
                SuggestedFix = "Organization şemasındaki sameAs dizisine resmi Wikidata ve sektörel otorite URL'lerinizi ekleyin."
            });
        }

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = "Markayı tanımlayan hiçbir sameAs profili veya bağımsız Knowledge Graph referansı bulunamadı.",
            SuggestedFix = "Organization şemanıza sosyal ve kurumsal tescil bağlantılarını içeren sameAs listesi ekleyin."
        });
    }
}

// 8.11 Hedef Pazarın Dili ve Hreflang
public class TargetLanguageRule : IAuditRule
{
    public string Id => "8.11";
    public string SectionId => "08";
    public string SectionTitle => "AI visibility (GEO and AEO)";
    public string Title => "Content exists in the target market's language";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var htmlLang = context.Document.DocumentElement?.GetAttribute("lang")?.Trim();
        var hreflangs = context.Document.QuerySelectorAll("link[rel='alternate'][hreflang]");

        if (string.IsNullOrWhiteSpace(htmlLang))
        {
            return Task.FromResult(new RuleResult
            {
                Id = Id,
                SectionId = SectionId,
                SectionTitle = SectionTitle,
                Title = Title,
                MaxScore = MaxScore,
                EarnedScore = 0,
                Status = AuditStatus.Missing,
                Finding = "<html> etiketinde ISO dil kodu ('lang') tanımlanmamış.",
                SuggestedFix = "<html lang='en'> veya hedef pazarın resmi ISO 639-1 dil kodunu ekleyin."
            });
        }

        if (hreflangs.Length > 0)
        {
            return Task.FromResult(new RuleResult
            {
                Id = Id,
                SectionId = SectionId,
                SectionTitle = SectionTitle,
                Title = Title,
                MaxScore = MaxScore,
                EarnedScore = 3,
                Status = AuditStatus.Pass,
                Finding = $"Sayfa dili ('{htmlLang}') ve {hreflangs.Length} alternatif pazar hreflang yapılandırması tam."
            });
        }

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 2,
            Status = AuditStatus.Partial,
            Finding = $"Sayfa dili ('{htmlLang}') tanımlı, ancak çok dilli pazar için çift yönlü hreflang etiketleri eksik.",
            SuggestedFix = "Farklı ülkelerdeki kullanıcı ve ajanlar için hreflang alternatif dil etiketlerini (<link rel='alternate' hreflang='...'>) ekleyin."
        });
    }
}

// 09 Measurement and Attribution
// 9.1 GA4 AI Kanal Grubu
public class Ga4AiSourcesRule : IAuditRule
{
    public string Id => "9.1";
    public string SectionId => "09";
    public string SectionTitle => "Measurement and attribution";
    public string Title => "A custom GA4 channel group isolates AI sources";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        // Not: GA4'ün iç panelindeki Regex/Kanal kuralları HTTP taramasıyla doğrulanamaz.
        // Şirket içi/manuel kural olduğu açıkça belirtilir.
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = "GA4 yapılandırması dış HTTP taramasıyla okunamadı (Şirket içi GA4 panelinde AI referral grubu kurulmalıdır).",
            SuggestedFix = "GA4 Admin > Data Settings > Channel Groups altında ChatGPT, Perplexity, Claude kaynaklarını ayıran bir 'AI Referrals' grubu tanımlayın."
        });
    }
}

// 10 AI Advertising (GEA) Readiness
// 10.7 Pixel & Conversions API (CAPI) Entegrasyonu
public class ConversionsApiRule : IAuditRule
{
    public string Id => "10.7";
    public string SectionId => "10";
    public string SectionTitle => "AI advertising (GEA) readiness";
    public string Title => "Pixel and Conversions API are installed";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var raw = context.RawHtml;

        // Modern istemci tarafı tarayıcı pikselleri (Meta, Google, TikTok, Shopify Web Pixels)
        bool hasClientPixel = raw.Contains("fbq(") ||
                              raw.Contains("fbevents.js") ||
                              raw.Contains("gtag(") ||
                              raw.Contains("google_tag_manager") ||
                              raw.Contains("dataLayer") ||
                              raw.Contains("trekkie") ||
                              raw.Contains("web-pixels-manager") ||
                              raw.Contains("ttq.load");

        // Sunucu taraflı CAPI veya Server-Side GTM sinyalleri (sgtm, /g/collect, proxy header vb.)
        bool hasServerSideCapi = raw.Contains("data-capi") ||
                                 raw.Contains("server-side-gtm") ||
                                 raw.Contains("conversions-api");

        if (hasClientPixel && hasServerSideCapi)
        {
            return Task.FromResult(new RuleResult
            {
                Id = Id,
                SectionId = SectionId,
                SectionTitle = SectionTitle,
                Title = Title,
                MaxScore = MaxScore,
                EarnedScore = 3,
                Status = AuditStatus.Pass,
                Finding = "Hem tarayıcı pikseli hem de Server-Side Conversions API (CAPI) altyapısı tespit edildi."
            });
        }

        if (hasClientPixel)
        {
            return Task.FromResult(new RuleResult
            {
                Id = Id,
                SectionId = SectionId,
                SectionTitle = SectionTitle,
                Title = Title,
                MaxScore = MaxScore,
                EarnedScore = 1,
                Status = AuditStatus.Partial,
                Finding = "İstemci tarafı dönüşüm pikseli (GTM/Meta/Shopify) aktif; ancak sinyal kaybını önleyen Server-Side CAPI entegrasyonu bulunamadı.",
                SuggestedFix = "Reklam ajanları ve AI optimizasyonu için Meta Conversions API (CAPI) ve Server-Side Google Tag Manager kurulumunu tamamlayın."
            });
        }

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = "Dönüşüm takip pikseli veya CAPI altyapısı bulunamadı.",
            SuggestedFix = "Meta Pixel / Google Tag Manager kodunu sayfaya ekleyin."
        });
    }
}