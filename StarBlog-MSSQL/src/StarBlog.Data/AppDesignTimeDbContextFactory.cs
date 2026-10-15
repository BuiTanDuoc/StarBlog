using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StarBlog.Data; 

public class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        // Chỉ định chuỗi kết nối qua biến môi trường CONNECTION_STRING, nếu chưa đặt thì dùng giá trị mặc định của SQL Server trên máy cục bộ
        var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                      ?? "Server=localhost;Database=StarBlog;Trusted_Connection=True;TrustServerCertificate=True;";

        builder.UseSqlServer(connStr);
        return new AppDbContext(builder.Options);
    }
}