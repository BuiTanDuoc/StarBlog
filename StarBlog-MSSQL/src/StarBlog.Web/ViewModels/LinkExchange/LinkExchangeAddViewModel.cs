using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels.LinkExchange;

public class LinkExchangeAddViewModel {
    /// <summary>
    /// Tên website
    /// </summary>
    [Display(Name = "Tên website")]
    [Required(ErrorMessage = "Phải nhập tên website")]
    public string Name { get; set; }

    /// <summary>
    /// Giới thiệu
    /// </summary>
    [Display(Name = "Giới thiệu")]
    public string? Description { get; set; }

    /// <summary>
    /// URL
    /// </summary>
    [Display(Name = "URL")]
    [Required(ErrorMessage = "Phải nhập URL")]
    [DataType(DataType.Url)]
    public string Url { get; set; }

    /// <summary>
    /// Quản trị viên website
    /// </summary>
    [Display(Name = "Tên quản trị viên website")]
    [Required(ErrorMessage = "Phải nhập tên quản trị viên website")]
    public string WebMaster { get; set; }

    /// <summary>
    /// Email liên hệ
    /// </summary>
    [Display(Name = "Email liên hệ")]
    [Required(ErrorMessage = "Phải nhập email liên hệ")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}