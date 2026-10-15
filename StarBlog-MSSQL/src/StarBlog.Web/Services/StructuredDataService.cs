using System.Text.Json;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Services;

public class StructuredDataService {
    private readonly ConfigService _configService;

    public StructuredDataService(ConfigService configService) {
        _configService = configService;
    }

    public string GetWebsiteStructuredData() {
        var baseUrl = GetBaseUrl();
        var data = new {
            context = "https://schema.org",
            type = "WebSite",
            name = "StarBlog",
            alternateName = "Blog Cao thủ vẽ ngôi sao",
            description = "Blog Cao thủ vẽ ngôi sao, Phòng thí nghiệm lập trình. Chuyên khám phá các công nghệ mới đang được quan tâm trên internet và thực hành phát triển linh hoạt theo nhóm.",
            url = baseUrl,
            author = new {
                type = "Person",
                name = "DealiAxy",
                url = "https://deali.cn"
            },
            publisher = new {
                type = "Organization",
                name = "StarBlog",
                url = baseUrl,
                logo = new {
                    type = "ImageObject",
                    url = $"{baseUrl}/favicon.ico"
                }
            },
            potentialAction = new {
                type = "SearchAction",
                target = new {
                    type = "EntryPoint",
                    urlTemplate = $"{baseUrl}/Search?q={{search_term_string}}"
                },
                queryInput = "required name=search_term_string"
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    public string GetOrganizationStructuredData() {
        var baseUrl = GetBaseUrl();
        var data = new {
            context = "https://schema.org",
            type = "Organization",
            name = "StarBlog",
            alternateName = "Blog Cao thủ vẽ ngôi sao",
            description = "Phòng thí nghiệm lập trình, chuyên khám phá các công nghệ mới đang được quan tâm trên internet và thực hành phát triển linh hoạt theo nhóm.",
            url = baseUrl,
            logo = new {
                type = "ImageObject",
                url = $"{baseUrl}/favicon.ico"
            },
            founder = new {
                type = "Person",
                name = "DealiAxy",
                url = "https://deali.cn"
            },
            sameAs = new[] {
                "https://github.com/Deali-Axy",
                "https://www.cnblogs.com/deali/",
                "https://www.zhihu.com/people/dealiaxy"
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    public string GetBlogPostingStructuredData(PostViewModel post) {
        var baseUrl = GetBaseUrl();
        var postUrl = post.Url ?? $"{baseUrl}/Blog/Post/{post.Id}";
        
        var data = new {
            context = "https://schema.org",
            type = "BlogPosting",
            headline = post.Title,
            description = !string.IsNullOrEmpty(post.Summary) && post.Summary != "(Không có mô tả)" 
                ? post.Summary 
                : ExtractDescriptionFromContent(post.Content),
            url = postUrl,
            datePublished = post.CreationTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            dateModified = post.LastUpdateTime.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            author = new {
                type = "Person",
                name = "DealiAxy",
                url = "https://deali.cn"
            },
            publisher = new {
                type = "Organization",
                name = "StarBlog",
                url = baseUrl,
                logo = new {
                    type = "ImageObject",
                    url = $"{baseUrl}/favicon.ico"
                }
            },
            mainEntityOfPage = new {
                type = "WebPage",
                id = postUrl
            },
            articleSection = post.Category?.Name,
            wordCount = post.Content?.Length ?? 0,
            inLanguage = "zh-CN"
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    public string GetBreadcrumbStructuredData(PostViewModel post) {
        var baseUrl = GetBaseUrl();
        var breadcrumbItems = new List<object> {
            new {
                type = "ListItem",
                position = 1,
                name = "Trang chủ",
                item = baseUrl
            },
            new {
                type = "ListItem",
                position = 2,
                name = "Blog",
                item = $"{baseUrl}/Blog/List"
            }
        };

        var position = 3;
        if (post.Categories != null && post.Categories.Any()) {
            foreach (var category in post.Categories) {
                breadcrumbItems.Add(new {
                    type = "ListItem",
                    position = position++,
                    name = category.Name,
                    item = $"{baseUrl}/Blog/List?categoryId={category.Id}"
                });
            }
        }

        breadcrumbItems.Add(new {
            type = "ListItem",
            position = position,
            name = post.Title,
            item = post.Url ?? $"{baseUrl}/Blog/Post/{post.Id}"
        });

        var data = new {
            context = "https://schema.org",
            type = "BreadcrumbList",
            itemListElement = breadcrumbItems
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    public string GetPersonStructuredData() {
        var data = new {
            context = "https://schema.org",
            type = "Person",
            name = "DealiAxy",
            alternateName = "Cao thủ vẽ ngôi sao",
            description = "Lập trình viên, blogger công nghệ, chuyên về phát triển Web, học máy và các lĩnh vực công nghệ khác.",
            url = "https://deali.cn",
            sameAs = new[] {
                "https://github.com/Deali-Axy",
                "https://www.cnblogs.com/deali/",
                "https://www.zhihu.com/people/dealiaxy",
                "https://live.bilibili.com/11883038"
            },
            jobTitle = "Kỹ sư phần mềm",
            worksFor = new {
                type = "Organization",
                name = "StarBlog"
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    private string ExtractDescriptionFromContent(string content) {
        if (string.IsNullOrEmpty(content)) return "StarBlog bài viết blog kỹ thuật";

        // Loại bỏ ký hiệu Markdown
        var plainText = System.Text.RegularExpressions.Regex.Replace(content, @"[#*`\[\]()_~]", "");
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"!\[.*?\]\(.*?\)", "");
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\[.*?\]\(.*?\)", "");
        
        // Lấy 160 ký tự đầu làm mô tả
        return plainText.Length > 160 ? plainText.Substring(0, 157) + "..." : plainText;
    }

    private string GetBaseUrl() {
        var host = _configService["host"];
        return !string.IsNullOrEmpty(host) ? host.TrimEnd('/') : "https://blog.deali.cn";
    }
}
