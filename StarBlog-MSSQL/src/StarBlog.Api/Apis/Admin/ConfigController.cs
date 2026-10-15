using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;

namespace StarBlog.Api.Apis.Admin;

/// <summary>
/// Trung tâm cấu hình
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public class ConfigController : ControllerBase {
    private readonly ConfigService _service;

    public ConfigController(ConfigService service) {
        _service = service;
    }

    [HttpGet]
    public List<ConfigItem> GetAll() {
        return _service.GetAll();
    }

    [HttpGet("{key}")]
    public ApiResponse<ConfigItem> GetByKey(string key) {
        var item = _service.GetByKey(key);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<ConfigItem>(item);
    }

    [HttpPost]
    public ApiResponse<ConfigItem> Add(ConfigItemCreationDto dto) {
        var item = _service.GetByKey(dto.Key);
        if (item != null) {
            return ApiResponse.BadRequest($"key {dto.Key} đã tồn tại!");
        }

        item = _service.AddOrUpdate(new ConfigItem {
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description
        });
        return new ApiResponse<ConfigItem>(item);
    }

    [HttpPut("{key}")]
    public ApiResponse<ConfigItem> Update(string key, ConfigItemUpdateDto dto) {
        var item = _service.GetByKey(key);
        if (item == null) return ApiResponse.NotFound();
        item.Value = dto.Value;
        item.Description = dto.Description;
        return new ApiResponse<ConfigItem>(_service.AddOrUpdate(item));
    }

    [HttpDelete("{key}")]
    public ApiResponse Delete(string key) {
        var item = _service.GetByKey(key);
        return item == null ? ApiResponse.NotFound() : ApiResponse.Ok($"Đã xóa {_service.DeleteByKey(key)} bản ghi");
    }
}