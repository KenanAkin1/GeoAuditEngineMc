using AngleSharp.Dom;
using GeoAuditEngineMc.Models;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace GeoAuditEngineMc.Rules.Section02;

// 2.1 Organization Schema (3 pts)
public class OrganizationSchemaRule : IAuditRule
{
    public string Id => "2.1";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Organization schema on the homepage";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var org = context.JsonLdNodes.FirstOrDefault(n =>
        {
            var raw = n.ToJsonString();
            return raw.Contains("\"Organization\"", StringComparison.OrdinalIgnoreCase) ||
                   raw.Contains("\"Corporation\"", StringComparison.OrdinalIgnoreCase) ||
                   raw.Contains("\"LocalBusiness\"", StringComparison.OrdinalIgnoreCase);
        });

        bool ok = org != null;
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = ok ? 3 : 0,
            Status = ok ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = ok ? "Organization/Corporation şeması başarıyla tespit edildi." : "Organization JSON-LD şeması bulunamadı.",
            SuggestedFix = ok ? null : "Marka tüzel kişiliği için Organization şeması ekleyin."
        });
    }
}

// 2.2 Product Schema (3 pts)
public class ProductSchemaRule : IAuditRule
{
    public string Id => "2.2";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Product schema on every product page";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var product = context.JsonLdNodes.FirstOrDefault(n => string.Equals(n["@type"]?.ToString(), "Product", StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = product != null ? 3 : 0,
            Status = product != null ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = product != null ? "Product şeması başarıyla tespit edildi." : "Product JSON-LD şeması bulunamadı."
        });
    }
}

// 2.3 Offer Schema (3 pts)
public class OfferSchemaRule : IAuditRule
{
    public string Id => "2.3";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Offer schema complete";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var product = context.JsonLdNodes.FirstOrDefault(n => string.Equals(n["@type"]?.ToString(), "Product", StringComparison.OrdinalIgnoreCase));
        var offer = product?["offers"];

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = offer != null ? 3 : 0,
            Status = offer != null ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = offer != null ? "Offer şeması tanımlı." : "Fiyat ve stok bildiren Offer şeması bulunamadı."
        });
    }
}

// 2.6 Reviews / AggregateRating (2 pts)
public class ReviewsSchemaRule : IAuditRule
{
    public string Id => "2.6";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "AggregateRating and Review reflect real reviews";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var rating = context.JsonLdNodes.FirstOrDefault(n => string.Equals(n["@type"]?.ToString(), "AggregateRating", StringComparison.OrdinalIgnoreCase) || n["aggregateRating"] != null);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = rating != null ? 2 : 0,
            Status = rating != null ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = rating != null ? "AggregateRating şeması bulundu." : "Yapılandırılmış kullanıcı yorumu/puan şeması bulunamadı."
        });
    }
}

// 2.7 FAQPage (1 pt)
public class FaqPageRule : IAuditRule
{
    public string Id => "2.7";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "FAQPage or QAPage on product and category pages";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var faq = context.JsonLdNodes.FirstOrDefault(n => string.Equals(n["@type"]?.ToString(), "FAQPage", StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = faq != null ? 1 : 0,
            Status = faq != null ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = faq != null ? "FAQPage JSON-LD şeması tespit edildi." : "Sıkça sorulan sorular için FAQPage şeması bulunamadı."
        });
    }
}

// 2.8 BreadcrumbList (1 pt)
public class BreadcrumbListRule : IAuditRule
{
    public string Id => "2.8";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "BreadcrumbList and a coherent category hierarchy";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var breadcrumb = context.JsonLdNodes.FirstOrDefault(n =>
            n.ToJsonString().Contains("\"BreadcrumbList\"", StringComparison.OrdinalIgnoreCase));

        bool ok = breadcrumb != null;
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = ok ? 1 : 0,
            Status = ok ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = ok ? "BreadcrumbList şeması doğrulandı." : "Sayfa kaynak kodunda BreadcrumbList JSON-LD şeması bulunamadı.",
            SuggestedFix = ok ? null : "Kategori hiyerarşisi için BreadcrumbList şemasını ekleyin."
        });
    }
}
// 2.9 Variants are modelled correctly (2 pts)
public class VariantsModelledRule : IAuditRule
{
    public string Id => "2.9";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Variants are modelled correctly";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var hasVariants = context.RawHtml.Contains("hasVariant") || context.RawHtml.Contains("ProductModel");
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasVariants ? 2 : 0,
            Status = hasVariants ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasVariants ? "Varyantlar Product/hasVariant şemasıyla modellenmiş." : "Varyant şema modellemesi tespit edilemedi."
        });
    }
}

// 2.11 JSON-LD Format (2 pts)
public class JsonLdFormatRule : IAuditRule
{
    public string Id => "2.11";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "JSON-LD is the format in use";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasJsonLd = context.JsonLdNodes.Count > 0;
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasJsonLd ? 2 : 0,
            Status = hasJsonLd ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasJsonLd ? $"{context.JsonLdNodes.Count} adet JSON-LD şeması kullanılıyor." : "Hiçbir JSON-LD bloğu bulunamadı."
        });
    }
}

// 2.12 Technical specifications are text, not images (3 pts)
public class TechnicalSpecsTextRule : IAuditRule
{
    public string Id => "2.12";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Technical specifications are text, not images";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        // 1. Resim afişi içine gömülmüş teknik tablo/özellik görseli var mı? (OCR gerektiren kötü pratik)
        var specImages = context.Document.QuerySelectorAll("img").Where(img =>
        {
            var src = (img.GetAttribute("src") ?? "").ToLowerInvariant();
            var alt = (img.GetAttribute("alt") ?? "").ToLowerInvariant();
            return src.Contains("spec") || src.Contains("tablo") || src.Contains("ozellik") ||
                   src.Contains("teknik") || alt.Contains("şartname") || alt.Contains("tablosu");
        }).ToList();

        // 2. Semantik HTML veri yapıları (Tablo, Tanım Listesi, Veri Listesi)
        var dataTables = context.Document.QuerySelectorAll("table, dl, .specs, .technical-data");
        var hasSemanticLists = context.Document.QuerySelectorAll("ul, ol").Any(l => l.Children.Length >= 3);

        // Sayfa metin hacmi yeterli ve özellikler resim olarak gömülmemişse
        if (specImages.Count == 0 && (dataTables.Length > 0 || hasSemanticLists))
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
                Finding = "Teknik ve operasyonel veriler resim içine gömülmeden, doğrudan taranabilir HTML veri yapıları (tablo/liste) ile sunuluyor."
            });
        }

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 1,
            Status = AuditStatus.Partial,
            Finding = "Teknik özellikler kısmen yapılandırılmamış; ajanların ayrıştırabileceği HTML tablosu veya semantik liste formatı eksik.",
            SuggestedFix = "Teknik verileri görsellerden çıkarıp <table> veya <dl> (tanım listesi) etiketleriyle taranabilir hale getirin."
        });
    }
}

// 2.13 Image alt text is descriptive and factual (2 pts)
public class ImageAltTextRule : IAuditRule
{
    public string Id => "2.13";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Image alt text is descriptive and factual";
    public int MaxScore => 2; // Doğru tavan puan: 2

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var images = context.Document.QuerySelectorAll("img");
        if (images.Length == 0)
        {
            return Task.FromResult(new RuleResult
            {
                Id = Id,
                SectionId = SectionId,
                SectionTitle = SectionTitle,
                Title = Title,
                MaxScore = MaxScore,
                EarnedScore = MaxScore,
                Status = AuditStatus.Pass,
                Finding = "Sayfada taranacak görsel bulunmuyor."
            });
        }

        int emptyAltCount = images.Count(img => string.IsNullOrWhiteSpace(img.GetAttribute("alt")));
        int filledAltCount = images.Length - emptyAltCount;

        // 1. Durum: Hepsi dolu -> TAM BAŞARI (2/2 pts)
        if (emptyAltCount == 0)
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
                Finding = $"Tüm görseller ({images.Length} adet) açıklayıcı alt etiketine sahip."
            });
        }

        // 2. Durum: Bazıları dolu, bazıları boş -> KISMİ BAŞARI (1/2 pts)
        if (filledAltCount > 0)
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
                Finding = $"{images.Length} görselden {emptyAltCount} tanesinde alt etiketi eksik veya boş ({filledAltCount} görselde tanımlı).",
                SuggestedFix = "Arama ve AI ajanlarının görselleri bağlamıyla anlayabilmesi için eksik olan görsellere de açıklayıcı alt metinleri girin."
            });
        }

        // 3. Durum: Hepsi boş -> BAŞARISIZ (0/2 pts)
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = 0,
            Status = AuditStatus.Missing,
            Finding = $"Sayfadaki {images.Length} görselin hiçbirinde alt etiketi bulunamadı.",
            SuggestedFix = "Tüm img etiketlerine somut ve açıklayıcı alt metinler ekleyin."
        });
    }
}

// 2.14 /llms.txt (1 pt)
public class LlmsTxtRule : IAuditRule
{
    public string Id => "2.14";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "/llms.txt is published";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasLlms = !string.IsNullOrWhiteSpace(context.LlmsTxt);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasLlms ? 1 : 0,
            Status = hasLlms ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasLlms ? "Kök dizinde /llms.txt tespit edildi." : "/llms.txt dosyası bulunamadı (404)."
        });
    }
}

// 2.16 Product/Service descriptions are fact-based (3 pts)
public class FactBasedDescriptionsRule : IAuditRule
{
    public string Id => "2.16";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Product descriptions are fact-based";
    public int MaxScore => 3;

    // Not: "%" ve tek harfli genel birimler (ör. tek başına "l") regex'ten çıkarıldı;
    // CSS/inline style içinde (width:100%, margin:20px vb.) sürekli yanlış pozitif üretiyorlardı.
    const string UniversalFactRegex =
        @"\b(\d+([.,]\d+)?\s*(kw|mw|kva|kv|bar|hz|m2|m3|cm|mm|km|kg|ton|lt|ml|₺|\$|€|usd|eur|gb|tb|mah|adet|paket|saat|gün|yıl))\b" +
        @"|\b(iso\s*\d{3,}|tse\s*\d+|iec\s*\d+|din\s*\d+|ce\s*belgeli)\b";

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var visibleText = GetVisibleText(context.Document.Body);

        var matches = Regex.Matches(visibleText, UniversalFactRegex, RegexOptions.IgnoreCase);

        // Aynı değerin defalarca tekrar etmesini (menü, footer, tekrarlanan blok) tek veri
        // noktası say; asıl amaç FARKLI olgusal referansların sayısı.
        var distinctFacts = matches
            .Select(m => m.Value.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (distinctFacts.Count >= 4)
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
                Finding = $"İçerikler soyut pazarlama sıfatları yerine doğrulanabilir olgusal metrikler barındırıyor ({distinctFacts.Count} farklı somut veri noktası tespit edildi)."
            });
        }

        if (distinctFacts.Count > 0)
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
                Finding = $"Sayfada birkaç ölçülebilir veri var ({distinctFacts.Count} adet) ancak genel içerik çoğunlukla soyut pazarlama iddialarından oluşuyor.",
                SuggestedFix = "Yapay zeka modellerinin referans alabilmesi için içeriğe somut kapasite, standart (ISO/TSE numarası), ölçü veya sayısal metrikler ekleyin."
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
            Finding = "Açıklamalar tamamen soyut pazarlama iddialarından oluşuyor; yapay zekanın doğrulayabileceği hiçbir teknik ölçü, kapasite veya standart bulunamadı.",
            SuggestedFix = "Sıfatları azaltın; projenin/ürünün/hizmetin somut teknik parametrelerini, kapasitesini ve standartlarını rakamlarla belirtin."
        });
    }

    // <script>, <style>, <noscript> içeriğini DIŞLAYARAK sadece kullanıcının gerçekten
    // gördüğü metni döndürür. Document üzerinde mutasyon yapmamak için klon üzerinde çalışır,
    // böylece bu rule'dan sonra çalışacak diğer rule'ların DOM'unu bozmaz.
    private static string GetVisibleText(IElement? body)
    {
        if (body == null) return string.Empty;

        var clone = (IElement)body.Clone(deep: true);
        foreach (var el in clone.QuerySelectorAll("script, style, noscript, template"))
        {
            el.Remove();
        }

        return clone.TextContent ?? string.Empty;
    }
}

// 2.17 Comparison and fit information
public class ComparisonInfoRule : IAuditRule
{
    public string Id => "2.17";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Comparison and fit information is present";
    public int MaxScore => 2; // Webtures standardı 2 puan

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var raw = context.RawHtml;
        bool hasFullTable = context.Document.QuerySelectorAll("table").Any(t => t.QuerySelectorAll("th").Length >= 3);
        bool hasServiceBlocks = context.Document.QuerySelectorAll("section, article, .service-item").Length >= 2;

        if (hasFullTable)
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
                Finding = "Detaylı karşılaştırma tablosu/matrisi doğrulandı."
            });
        }

        if (hasServiceBlocks || raw.Contains("hizmet") || raw.Contains("paket"))
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
                Finding = "Hizmet/ürün seçenekleri listelenmiş ancak karşılaştırmalı bir karar matrisi bulunmuyor."
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
            Finding = "Karşılaştırma veya seçim rehberi bulunamadı."
        });
    }
}

// 2.20 Semantic HTML (3 pts)
public class SemanticHtmlRule : IAuditRule
{
    public string Id => "2.20";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Semantic HTML in use; headings, lists and sections are marked up";
    public int MaxScore => 3;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var h1Count = context.Document.QuerySelectorAll("h1").Length;
        var hasSemantic = context.Document.QuerySelector("main, article, section, nav, header") != null;
        bool ok = h1Count == 1 && hasSemantic;

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = ok ? 3 : 1,
            Status = ok ? AuditStatus.Pass : AuditStatus.Partial,
            Finding = ok ? "Tek H1 ve semantik HTML (<main>, <section>) başarıyla uygulandı." : $"H1 sayısı: {h1Count}. Semantik etiketler zayıf veya birden fazla H1 var."
        });
    }
}

// 2.21 Unique Meta Description (2 pts)
public class MetaDescriptionRule : IAuditRule
{
    public string Id => "2.21";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "A unique meta description on every page";
    public int MaxScore => 2;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var metaDesc = context.Document.QuerySelector("meta[name='description']");
        var content = metaDesc?.GetAttribute("content")?.Trim();
        bool ok = !string.IsNullOrWhiteSpace(content);

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = ok ? 2 : 0,
            Status = ok ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = ok ? $"Meta description tespit edildi ({content!.Length} karakter)." : "Meta description etiketi bulunamadı veya boş."
        });
    }
}

// 2.22 Open Graph Tags (1 pt)
public class OpenGraphRule : IAuditRule
{
    public string Id => "2.22";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "Open Graph and social sharing tags defined";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        var ogTitle = context.Document.QuerySelector("meta[property='og:title']");
        var ogImage = context.Document.QuerySelector("meta[property='og:image']");
        bool ok = ogTitle != null && ogImage != null;

        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = ok ? 1 : 0,
            Status = ok ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = ok ? "Open Graph (og:title, og:image) etiketleri tanımlı." : "og:title veya og:image meta etiketleri eksik."
        });
    }
}

// 2.23 AGENTS.md (1 pt)
public class AgentFilesRule : IAuditRule
{
    public string Id => "2.23";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "AGENTS.md published and current";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasAgents = !string.IsNullOrWhiteSpace(context.AgentsMd);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasAgents ? 1 : 0,
            Status = hasAgents ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasAgents ? "Kök dizinde /AGENTS.md bulundu." : "/AGENTS.md dosyası eksik."
        });
    }
}

// 2.24 skill.md (1 pt)
public class SkillMdRule : IAuditRule
{
    public string Id => "2.24";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "skill.md / agent capability definition considered";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasSkill = !string.IsNullOrWhiteSpace(context.SkillMd);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasSkill ? 1 : 0,
            Status = hasSkill ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasSkill ? "Kök dizinde /skill.md tespit edildi." : "Ajan yetenek tanımı (/skill.md) eksik."
        });
    }
}

// 2.25 llms-full.txt (1 pt)
public class LlmsFullTxtRule : IAuditRule
{
    public string Id => "2.25";
    public string SectionId => "02";
    public string SectionTitle => "Machine readability and structured data";
    public string Title => "llms-full.txt full-content version served";
    public int MaxScore => 1;

    public Task<RuleResult> EvaluateAsync(AuditContext context)
    {
        bool hasFull = !string.IsNullOrWhiteSpace(context.LlmsFullTxt);
        return Task.FromResult(new RuleResult
        {
            Id = Id,
            SectionId = SectionId,
            SectionTitle = SectionTitle,
            Title = Title,
            MaxScore = MaxScore,
            EarnedScore = hasFull ? 1 : 0,
            Status = hasFull ? AuditStatus.Pass : AuditStatus.Missing,
            Finding = hasFull ? "Kök dizinde /llms-full.txt bulundu." : "/llms-full.txt dosyası bulunamadı."
        });
    }
}