using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels.Photography;

public class PhotoCreationDto {
    /// <summary>
    /// Tiêu đề tác phẩm
    /// </summary>
    [Required(ErrorMessage = "Tiêu đề tác phẩm không được để trống")]
    public string Title { get; set; }

    /// <summary>
    /// Địa điểm chụp
    /// </summary>
    [Required(ErrorMessage = "Địa điểm chụp không được để trống")]
    public string Location { get; set; }
}