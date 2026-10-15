using System.Text.Json;
using CodeLab.Share.Contrib.StopWords;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels.Comments;
using StarBlog.Application.Criteria;

namespace StarBlog.Api.Apis.Comments;

[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Comment)]
public class CommentController : ControllerBase {
    private readonly CommentService _commentService;
    private readonly TempFilterService _filter;

    public CommentController(CommentService commentService, TempFilterService filter) {
        _commentService = commentService;
        _filter = filter;
    }

    /// <summary>
    /// Lấy bình luận có phân trang
    /// </summary>
    [HttpGet]
    public async Task<ApiResponsePaged<Comment>> GetPagedList([FromQuery] CommentQueryParameters @params) {
        var adminMode = User.Identity?.IsAuthenticated ?? false;
        var (data, meta) = await _commentService.GetPagedList(@params, adminMode, adminMode);
        return new ApiResponsePaged<Comment>(data, meta);
    }

    /// <summary>
    /// Lấy thông tin người dùng ẩn danh dựa trên email và mã xác thực
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetAnonymousUser(string email, string otp) {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        if (!CommentService.IsValidEmail(email)) return ApiResponse.BadRequest("Địa chỉ email được cung cấp không hợp lệ");

        var verified = _commentService.VerifyOtp(email, otp, clear: false);
        if (!verified) return ApiResponse.BadRequest("Mã xác thực không hợp lệ");

        var anonymous = await _commentService.GetAnonymousUser(email);
        // Tạm thời không dùng chức năng sinh mã xác thực mới, tránh trải nghiệm người dùng bị đứt gãy
        // var (_, newOtp) = await _commentService.GenerateOtp(email, true);

        return ApiResponse.Ok(new {
            AnonymousUser = anonymous,
            NewOtp = otp
        });
    }

    /// <summary>
    /// Lấy mã xác thực qua email
    /// </summary>
    [HttpGet("[action]")]
    public async Task<ApiResponse> GetEmailOtp(string email) {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";

        if (!CommentService.IsValidEmail(email)) {
            return ApiResponse.BadRequest("Địa chỉ email được cung cấp không hợp lệ");
        }

        var (result, _) = await _commentService.GenerateOtp(email);
        return result
            ? ApiResponse.Ok("Gửi mã xác thực qua email thành công, có hiệu lực trong năm phút")
            : ApiResponse.BadRequest("Mã xác thực trước đó vẫn còn hiệu lực, vui lòng không yêu cầu mã lặp lại");
    }

    [HttpPost]
    public async Task<ApiResponse<Comment>> Add(CommentCreationDto dto) {
        if (!_commentService.VerifyOtp(dto.Email, dto.EmailOtp)) {
            return ApiResponse.BadRequest("Mã xác thực không hợp lệ");
        }

        var anonymousUser = await _commentService.GetOrCreateAnonymousUser(
            dto.UserName, dto.Email, dto.Url,
            HttpContext.GetRemoteIpAddress()?.ToString().Split(":")?.Last()
        );

        var comment = new Comment {
            ParentId = dto.ParentId,
            PostId = dto.PostId,
            AnonymousUserId = anonymousUser.Id,
            UserAgent = Request.Headers.UserAgent,
            Content = dto.Content
        };

        string msg;
        if (_filter.CheckBadWord(dto.Content)) {
            comment.IsNeedAudit = true;
            comment.Visible = false;
            msg = "Trợ lý kiểm duyệt phát hiện bạn có thể đã dùng từ ngữ không phù hợp, bình luận này sẽ được hiển thị sau khi được duyệt~";
        }
        else {
            comment.Visible = true;
            msg = "Bình luận đã được trợ lý kiểm duyệt phê duyệt, cảm ơn bạn đã tham gia thảo luận~";
        }

        comment = await _commentService.Add(comment);
        if (comment.ParentId != null && comment.Visible) {
            await _commentService.EnqueueReplyNotificationIfNeededAsync(comment);
        }

        return new ApiResponse<Comment>(comment) {
            Message = msg
        };
    }

    [HttpGet("[action]")]
    public async Task<List<Comment>?> GetAll(string postId) {
        return await _commentService.GetAll(postId);
    }

    /// <summary>
    /// Duyệt
    /// </summary>
    [Authorize]
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<Comment>> Accept([FromRoute] string id, [FromBody] CommentAcceptDto dto) {
        var item = await _commentService.GetById(id);
        if (item == null) return ApiResponse.NotFound();
        return new ApiResponse<Comment>(await _commentService.Accept(item, dto.Reason));
    }

    /// <summary>
    /// Từ chối duyệt
    /// </summary>
    [Authorize]
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<Comment>> Reject([FromRoute] string id, [FromBody] CommentRejectDto dto) {
        var item = await _commentService.GetById(id);
        if (item == null) return ApiResponse.NotFound();
        return new ApiResponse<Comment>(await _commentService.Reject(item, dto.Reason));
    }
}
