using System.Text;
using System.Xml;
using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

/// <summary>
/// Dịch vụ tạo Sitemap thống nhất
/// </summary>
public class SitemapService {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly ConfigService _configService;
    private readonly ImageSeoService _imageSeoService;

    public SitemapService(
        IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        ConfigService configService,
        ImageSeoService imageSeoService) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _configService = configService;
        _imageSeoService = imageSeoService;
    }

    /// <summary>
    /// Tạo sitemap chính
    /// </summary>
    public async Task<string> GenerateMainSitemapAsync() {
        var baseUrl = GetBaseUrl();
        var urls = new List<SitemapUrl>();

        // Thêm trang chủ
        urls.Add(new SitemapUrl {
            Location = baseUrl,
            LastModified = DateTime.Now,
            ChangeFrequency = "daily",
            Priority = "1.0"
        });

        // Thêm trang danh sách blog
        urls.Add(new SitemapUrl {
            Location = $"{baseUrl}/Blog/List",
            LastModified = DateTime.Now,
            ChangeFrequency = "daily",
            Priority = "0.8"
        });

        // Thêm các trang danh mục
        var categories = await _categoryRepo.Where(c => c.Visible).ToListAsync();
        foreach (var category in categories) {
            urls.Add(new SitemapUrl {
                Location = $"{baseUrl}/Blog/List?categoryId={category.Id}",
                LastModified = DateTime.Now,
                ChangeFrequency = "weekly",
                Priority = "0.7"
            });
        }

        // Thêm các trang bài viết
        var posts = await _postRepo.Where(p => p.IsPublish)
            .OrderByDescending(p => p.LastUpdateTime)
            .ToListAsync();

        foreach (var post in posts) {
            var postUrl = string.IsNullOrWhiteSpace(post.Slug)
                ? $"{baseUrl}/Blog/Post/{post.Id}"
                : $"{baseUrl}/p/{post.Slug}";

            urls.Add(new SitemapUrl {
                Location = postUrl,
                LastModified = post.LastUpdateTime,
                ChangeFrequency = "monthly",
                Priority = "0.6"
            });
        }

        // Thêm các trang tĩnh khác
        var staticPages = new[] {
            new { Url = "/About", Priority = "0.5", ChangeFreq = "monthly" },
            new { Url = "/Photography", Priority = "0.5", ChangeFreq = "weekly" },
            new { Url = "/LinkExchange", Priority = "0.4", ChangeFreq = "monthly" }
        };

        foreach (var page in staticPages) {
            urls.Add(new SitemapUrl {
                Location = $"{baseUrl}{page.Url}",
                LastModified = DateTime.Now,
                ChangeFrequency = page.ChangeFreq,
                Priority = page.Priority
            });
        }

        return GenerateXmlSitemap(urls);
    }

    /// <summary>
    /// Tạo sitemap hình ảnh
    /// </summary>
    public async Task<string> GenerateImageSitemapAsync() {
        var baseUrl = GetBaseUrl();
        var posts = await _postRepo.Where(p => p.IsPublish).ToListAsync();
        var imageUrls = new List<SitemapImageUrl>();

        foreach (var post in posts) {
            var postImageUrls = _imageSeoService.ExtractImageUrls(post);
            if (!postImageUrls.Any()) continue;

            var postUrl = string.IsNullOrWhiteSpace(post.Slug)
                ? $"{baseUrl}/Blog/Post/{post.Id}"
                : $"{baseUrl}/p/{post.Slug}";

            foreach (var imageUrl in postImageUrls) {
                imageUrls.Add(new SitemapImageUrl {
                    PageUrl = postUrl,
                    ImageUrl = imageUrl,
                    Caption = $"{post.Title} hình ảnh liên quan",
                    Title = post.Title,
                    LastModified = post.LastUpdateTime
                });
            }
        }

        return GenerateXmlImageSitemap(imageUrls);
    }

    /// <summary>
    /// Tạo chỉ mục sitemap
    /// </summary>
    public string GenerateSitemapIndex() {
        var baseUrl = GetBaseUrl();
        var sitemaps = new List<SitemapIndexEntry> {
            new() {
                Location = $"{baseUrl}/sitemap.xml",
                LastModified = DateTime.Now
            },
            new() {
                Location = $"{baseUrl}/sitemap-images.xml",
                LastModified = DateTime.Now
            }
        };

        return GenerateXmlSitemapIndex(sitemaps);
    }

    /// <summary>
    /// Tạo XML chuẩn sitemap
    /// </summary>
    private string GenerateXmlSitemap(IEnumerable<SitemapUrl> urls) {
        var settings = CreateXmlWriterSettings();
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

        foreach (var url in urls) {
            xmlWriter.WriteStartElement("url");
            xmlWriter.WriteElementString("loc", url.Location);
            xmlWriter.WriteElementString("lastmod", url.LastModified.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            xmlWriter.WriteElementString("changefreq", url.ChangeFrequency);
            xmlWriter.WriteElementString("priority", url.Priority);
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return stringWriter.ToString();
    }

    /// <summary>
    /// Tạo XML hình ảnh sitemap
    /// </summary>
    private string GenerateXmlImageSitemap(IEnumerable<SitemapImageUrl> imageUrls) {
        var settings = CreateXmlWriterSettings();
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
        xmlWriter.WriteAttributeString("xmlns", "image", null, "http://www.google.com/schemas/sitemap-image/1.1");

        var groupedImages = imageUrls.GroupBy(img => img.PageUrl);
        foreach (var group in groupedImages) {
            xmlWriter.WriteStartElement("url");
            xmlWriter.WriteElementString("loc", group.Key);

            foreach (var image in group) {
                xmlWriter.WriteStartElement("image", "image", "http://www.google.com/schemas/sitemap-image/1.1");
                xmlWriter.WriteElementString("image", "loc", "http://www.google.com/schemas/sitemap-image/1.1", image.ImageUrl);
                xmlWriter.WriteElementString("image", "caption", "http://www.google.com/schemas/sitemap-image/1.1", image.Caption);
                xmlWriter.WriteElementString("image", "title", "http://www.google.com/schemas/sitemap-image/1.1", image.Title);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return stringWriter.ToString();
    }

    /// <summary>
    /// Tạo XML chỉ mục sitemap
    /// </summary>
    private string GenerateXmlSitemapIndex(IEnumerable<SitemapIndexEntry> sitemaps) {
        var settings = CreateXmlWriterSettings();
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/0.9");

        foreach (var sitemap in sitemaps) {
            xmlWriter.WriteStartElement("sitemap");
            xmlWriter.WriteElementString("loc", sitemap.Location);
            xmlWriter.WriteElementString("lastmod", sitemap.LastModified.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        return stringWriter.ToString();
    }

    /// <summary>
    /// Tạo thiết lập ghi XML thống nhất
    /// </summary>
    private static XmlWriterSettings CreateXmlWriterSettings() {
        return new XmlWriterSettings {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };
    }

    /// <summary>
    /// Lấy URL cơ sở
    /// </summary>
    private string GetBaseUrl() {
        var host = _configService["host"];
        return !string.IsNullOrEmpty(host) ? host.TrimEnd('/') : "https://blog.deali.cn";
    }
}

/// <summary>
/// Sitemap Mục URL
/// </summary>
public class SitemapUrl {
    public string Location { get; set; } = "";
    public DateTime LastModified { get; set; }
    public string ChangeFrequency { get; set; } = "";
    public string Priority { get; set; } = "";
}

/// <summary>
/// Mục URL hình ảnh của Sitemap
/// </summary>
public class SitemapImageUrl {
    public string PageUrl { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Caption { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Mục chỉ mục Sitemap
/// </summary>
public class SitemapIndexEntry {
    public string Location { get; set; } = "";
    public DateTime LastModified { get; set; }
}
