using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels.Comments;

public class CommentCreationDto {
    public string? ParentId { get; set; }

    [Required]
    public string PostId { get; set; }

    [MinLength(2, ErrorMessage = "Độ dài từ 2 đến 20 ký tự")]
    [MaxLength(20, ErrorMessage = "Độ dài từ 2 đến 20 ký tự")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Địa chỉ email không được để trống")]
    [EmailAddress(ErrorMessage = "Định dạng email không đúng")]
    public string Email { get; set; }

    [Url(ErrorMessage = "Vui lòng nhập đúng url")]
    public string? Url { get; set; }

    [Required(ErrorMessage = "Mã xác thực email không được để trống")]
    [StringLength(4, ErrorMessage = "Độ dài phải là 4 ký tự")]
    public string EmailOtp { get; set; }

    [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
    [MaxLength(300, ErrorMessage = "Độ dài tối đa của bình luận là 300 ký tự")]
    public string Content { get; set; }
}