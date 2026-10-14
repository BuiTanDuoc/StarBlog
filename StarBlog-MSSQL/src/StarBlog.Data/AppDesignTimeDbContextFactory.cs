using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StarBlog.Data; 

public class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext> {
    public AppDbContext CreateDbContext(string[] args) {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        // 通过环境变量 CONNECTION_STRING 指定连接字符串，未设置时使用本机 SQL Server 默认值
        var connStr = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                      ?? "Server=localhost;Database=StarBlog;Trusted_Connection=True;TrustServerCertificate=True;";

        builder.UseSqlServer(connStr);
        return new AppDbContext(builder.Options);
    }
}