using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Photography;

namespace StarBlog.Web.Controllers;

public class PhotographyController : Controller {
    private readonly PhotoService _photoService;
    private readonly MessageService _messages;

    public PhotographyController(PhotoService photoService, MessageService messages) {
        _photoService = photoService;
        _messages = messages;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10) {
        return View(new PhotographyViewModel {
            Photos = await _photoService.GetPagedList(page, pageSize)
        });
    }

    public async Task<IActionResult> Photo(string id) {
        var photo = await _photoService.GetById(id);
        if (photo == null) {
            _messages.Error($"Hình ảnh {id} không tồn tại!");
            return RedirectToAction(nameof(Index));
        }

        return View(photo);
    }

    public async Task<IActionResult> Next(string id) {
        var item = await _photoService.GetNext(id);
        if (item == null) {
            _messages.Warning("Không còn ảnh tiếp theo nữa~");
            return RedirectToAction(nameof(Photo), new {id});
        }

        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }

    public async Task<IActionResult> Previous(string id) {
        var item = await _photoService.GetPrevious(id);
        if (item == null) {
            _messages.Warning("Không còn ảnh trước đó nữa~");
            return RedirectToAction(nameof(Photo), new {id});
        }

        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }

    public async Task<IActionResult> RandomPhoto() {
        var item = await _photoService.GetRandomPhoto();
        if (item == null) {
            _messages.Error("Hiện chưa có hình ảnh nào, vui lòng tải lên trước!");
            return RedirectToAction("Index", "Home");
        }

        _messages.Info($"Đã đề xuất ngẫu nhiên hình ảnh <b>{item.Title}</b> cho bạn~" +
                       $"<span class='ps-3'><a href=\"{Url.Action(nameof(RandomPhoto))}\">Thử lại lần nữa</a></span>");
        return RedirectToAction(nameof(Photo), new {id = item.Id});
    }
}