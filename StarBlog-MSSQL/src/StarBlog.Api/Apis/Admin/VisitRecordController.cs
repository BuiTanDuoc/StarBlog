using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services.VisitRecordServices;
using StarBlog.Application.Criteria;
using StarBlog.Application.ViewModels.VisitRecord;

namespace StarBlog.Api.Apis.Admin;

/// <summary>
/// Bản ghi truy cập
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public class VisitRecordController : ControllerBase {
    private readonly VisitRecordAnalyticsService _service;

    public VisitRecordController(VisitRecordAnalyticsService service) {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResponsePaged<VisitRecord>> GetList([FromQuery] VisitRecordParameters param) {
        var pagedList = await _service.GetPagedList(param);
        return new ApiResponsePaged<VisitRecord>(pagedList);
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<VisitRecord>> GetById(int id) {
        var item = await _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : item;
    }

    /// <summary>
    /// Lấy toàn bộ bản ghi truy cập
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<ApiResponse<List<VisitRecord>>> GetAll([FromQuery] VisitRecordParameters p) {
        return await _service.GetAll(p);
    }

    /// <summary>
    /// Dữ liệu tổng quan
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse<VisitOverview>> Overview([FromQuery] VisitRecordParameters p) {
        return await _service.Overview(p);
    }

    /// <summary>
    /// Dữ liệu xu hướng
    /// </summary>
    /// <param name="days">Xem dữ liệu của vài ngày gần đây, mặc định 7 ngày</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<DailyTrend>>> DailyTrend([FromQuery] VisitRecordParameters p, int days = 7) {
        return await _service.GetDailyTrend(p, days);
    }
    
    /// <summary>
    /// Lấy xu hướng theo giờ
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<HourlyTrend>>> HourlyTrend([FromQuery] VisitRecordParameters p) {
        return await _service.GetHourlyTrend(p);
    }

    /// <summary>
    /// Lấy tham số lọc thông tin địa lý
    /// </summary>
    /// <param name="param">Tùy chọn country, province, city</param>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<string?>>> GeoFilterParams([FromQuery] VisitRecordParameters p, string param = "country") {
        return await _service.GetGeoFilterParams(p, param);
    }

    /// <summary>
    /// Lấy tham số lọc UserAgent
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<UserAgentFilterParams>> UserAgentFilterParams([FromQuery] VisitRecordParameters p) {
        return await _service.GetUserAgentFilterParams(p);
    }

    /// <summary>
    /// Thống kê phân bố vị trí địa lý
    /// </summary>
    [HttpGet("GeoDistribution")]
    public async Task<ApiResponse> GeoDistribution([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetGeoDistribution(p));
    }

    /// <summary>
    /// Lấy phân tích tên miền nguồn
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<ReferrerDomain>>> ReferrerDomains([FromQuery] VisitRecordParameters p, int top = 10) {
        return await _service.GetReferrerDomains(p, top);
    }

    /// <summary>
    /// Lấy dữ liệu tăng trưởng so với kỳ trước
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<GrowthRate>> GrowthRate([FromQuery] VisitRecordParameters p, int days = 7) {
        return await _service.GetGrowthRate(p, days);
    }

    /// <summary>
    /// Lấy thống kê tỷ lệ thoát
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<BounceRateStats>> BounceRate([FromQuery] VisitRecordParameters p) {
        return await _service.GetBounceRate(p);
    }

    /// <summary>
    /// Lấy thống kê lần truy cập đầu tiên
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<FirstVisitStats>> FirstVisit([FromQuery] VisitRecordParameters p, [FromQuery] int days = 7) {
        return await _service.GetFirstVisitStats(p, days);
    }

    /// <summary>
    /// Lấy thống kê phân bố công nghệ
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<TechDistribution>> TechDistribution([FromQuery] VisitRecordParameters p) {
        return await _service.GetTechDistribution(p);
    }

    /// <summary>
    /// Lấy xếp hạng request chậm
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<SlowRequest>>> SlowRequests([FromQuery] VisitRecordParameters p, int top = 10) {
        return await _service.GetSlowRequests(p, top);
    }

    /// <summary>
    /// Lấy phân tích hành vi chuyển đổi
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<ConversionStats>> Conversion([FromQuery] VisitRecordParameters p, string targetPath) {
        return await _service.GetConversionStats(p, targetPath);
    }

    /// <summary>
    /// Lấy thống kê PV/UV
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<PvUv>> PvUv([FromQuery] VisitRecordParameters p) {
        return await _service.GetPvUv(p);
    }

    /// <summary>
    /// Lấy thống kê thời gian phản hồi
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> ResponseTimeStats([FromQuery] VisitRecordParameters p) {
        return ApiResponse.Ok(await _service.GetResponseTimeStats(p));
    }

    /// <summary>
    /// Lấy Top N đường dẫn truy cập
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse<List<TopPath>>> TopPaths([FromQuery] VisitRecordParameters p, int top = 10) {
        return await _service.GetTopPaths(p, top);
    }
}
