using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels;

public class InitViewModel {
    [Display(Name = "Tên người dùng")]
    public string Username { get; set; }

    [Display(Name = "Mật khẩu")]
    public string Password { get; set; }

    [Display(Name = "Tên miền blog")]
    public string Host { get; set; }

    [Display(Name = "Cách render bài viết mặc định")]
    public string DefaultRender { get; set; }
}