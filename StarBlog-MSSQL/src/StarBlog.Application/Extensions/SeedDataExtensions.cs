using FreeSql;
using StarBlog.Content.Extensions;
using StarBlog.Data.Models;

namespace StarBlog.Application.Extensions;

/// <summary>
/// Dữ liệu khởi tạo (seed) chạy khi ứng dụng khởi động.
/// </summary>
public static class SeedDataExtensions {
    /// <summary>
    /// Tạo sẵn tài khoản quản trị nếu bảng user đang rỗng, để không cần vào trang /Home/Init.
    /// Cấu hình tại mục <c>Seed:Admin</c> trong appsettings.json (Enabled / Username / Password).
    /// Chỉ chạy khi chưa có bất kỳ người dùng nào, nên an toàn khi khởi động lại nhiều lần.
    /// </summary>
    public static void SeedAdminUser(this IServiceProvider services) {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");

        try {
            var section = sp.GetRequiredService<IConfiguration>().GetSection("Seed:Admin");
            if (!section.GetValue("Enabled", false)) return;

            var username = section["Username"];
            var password = section["Password"];
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) {
                logger.LogWarning("Seed:Admin đã bật nhưng thiếu Username hoặc Password, bỏ qua việc tạo tài khoản quản trị");
                return;
            }

            var userRepo = sp.GetRequiredService<IBaseRepository<User>>();
            if (userRepo.Select.Any()) return; // Đã có người dùng, không seed nữa

            userRepo.Insert(new User {
                Id = Guid.NewGuid().ToString(),
                Name = username,
                Password = password.ToSHA256() // Cùng cách băm với trang Init và API đăng nhập
            });

            // Đánh dấu đã khởi tạo để đóng trang /Home/Init (tránh người lạ tạo thêm tài khoản quản trị)
            var configRepo = sp.GetRequiredService<IBaseRepository<ConfigItem>>();
            var isInit = configRepo.Where(a => a.Key == "is_init").First();
            if (isInit == null) {
                configRepo.Insert(new ConfigItem { Key = "is_init", Value = "true", Description = "Seed" });
            }
            else if (isInit.Value != "true") {
                isInit.Value = "true";
                configRepo.Update(isInit);
            }

            logger.LogWarning("Đã tạo tài khoản quản trị mặc định '{Username}'. Hãy đổi mật khẩu mặc định trong Seed:Admin trước khi triển khai thật!", username);
        }
        catch (Exception ex) {
            // Không để lỗi seed làm ứng dụng không khởi động được
            logger.LogError(ex, "Seed tài khoản quản trị thất bại");
        }
    }
}
