using System.ComponentModel.DataAnnotations;

namespace StarBlog.Application.ViewModels.Photography;

public class PhotoUpdateDto {
    public string? Id { get; set; }
    
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

    public DateTime CreateTime { get; set; }
}