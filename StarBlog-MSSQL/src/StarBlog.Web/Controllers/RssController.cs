using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Services;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class RssController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly ConfigService _conf;

    public RssController(IBaseRepository<Post> postRepo, ConfigService conf) {
        _postRepo = postRepo;
        _conf = conf;
    }

    [HttpGet]
    public IActionResult Index() {
        var feedUrl = Path.Combine(_conf["host"], "feed");
        ViewBag.FeedUrl = feedUrl;
        return View();
    }

    [ResponseCache(Duration = 1200)]
    [HttpGet("feed")]
    public async Task<IActionResult> Feed() {
        var posts = await _postRepo.Where(a => a.IsPublish && a.CreationTime.Year == DateTime.Now.Year)
            .OrderByDescending(a => a.LastUpdateTime)
            .Include(a => a.Category)
            .ToListAsync();

        var feed = new SyndicationFeed(
            "StarBlog",
            "Phòng thí nghiệm lập trình chuyên khám phá các công nghệ mới đang được quan tâm trên internet và thực hành phát triển linh hoạt theo nhóm, bao gồm thiết kế kiến trúc, thuật toán học máy và phân tích dữ liệu, phát triển di động, Linux, phát triển Web frontend/backend, v.v., hoan nghênh cùng thảo luận kỹ thuật, chia sẻ kinh nghiệm học tập và thực hành.",
            new Uri("http://blog.deali.cn"), "RSSUrl", posts.First().LastUpdateTime
        ) {
            Copyright = new TextSyndicationContent($"{DateTime.Now.Year} DealiAxy")
        };

        var items = new List<SyndicationItem>();
        foreach (var item in posts) {
            var postUrl = Url.Action("Post", "Blog", new {id = item.Id}, HttpContext.Request.Scheme);
            items.Add(new SyndicationItem(item.Title,
                new TextSyndicationContent(PostService.GetContentHtml(item), TextSyndicationContentKind.Html),
                new Uri(postUrl), item.Id, item.LastUpdateTime
            ) {
                Categories = {new SyndicationCategory(item.Category?.Name)},
                Authors = {new SyndicationPerson("admin@deali.cn", "DealiAxy", "https://deali.cn")},
                PublishDate = item.CreationTime,
                Summary = new TextSyndicationContent(item.Summary)
            });
        }

        feed.Items = items;

        var settings = new XmlWriterSettings {
            Async = true,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };
        using var stream = new MemoryStream();
        await using var xmlWriter = XmlWriter.Create(stream, settings);
        var rssFormatter = new Atom10FeedFormatter(feed);
        rssFormatter.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();

        return File(stream.ToArray(), "application/xml; charset=utf-8");
    }
}