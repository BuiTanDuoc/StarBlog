using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.LinkExchange;

namespace StarBlog.Web.Controllers;

public class LinkExchangeController : Controller {
    private readonly ILogger<LinkExchangeController> _logger;
    private readonly LinkExchangeService _service;
    private readonly IMapper _mapper;
    private readonly MessageService _messages;

    public LinkExchangeController(ILogger<LinkExchangeController> logger, LinkExchangeService service, IMapper mapper,
        MessageService messages) {
        _logger = logger;
        _service = service;
        _mapper = mapper;
        _messages = messages;
    }

    [HttpGet]
    public IActionResult Add() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(LinkExchangeAddViewModel vm) {
        if (!ModelState.IsValid) return View();

        if (await _service.HasUrl(vm.Url)) {
            _messages.Error("Yêu cầu trao đổi liên kết cho cùng URL đã được gửi!");
            return View();
        }

        var item = _mapper.Map<LinkExchange>(vm);
        item = await _service.AddOrUpdate(item);

        // Gửi email thông báo
        await _service.SendEmailOnAdd(item);

        _messages.Info("Yêu cầu trao đổi liên kết đã được gửi, đang được xử lý, vui lòng theo dõi thông báo qua email~");
        return RedirectToAction("Index", "Home");
    }
}