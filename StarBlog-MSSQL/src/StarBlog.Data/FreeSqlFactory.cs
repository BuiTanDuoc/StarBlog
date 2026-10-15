using System.Diagnostics;
using FreeSql;
using FreeSql.Internal;

namespace StarBlog.Data;

public class FreeSqlFactory {
    public static IFreeSql Create(DataType dataType, string connectionString) {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(dataType, connectionString)
            .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
            .UseAutoSyncStructure(true) //Tự động đồng bộ cấu trúc entity vào cơ sở dữ liệu
            .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
            .Build(); //Nhất định phải định nghĩa dưới dạng Singleton

        // FreeSql Với thuộc tính string chưa chỉ định độ dài, mặc định sinh nvarchar(255).
        // SQLite không kiểm tra độ dài nên trước giờ không sao, nhưng SQL Server sẽ báo lỗi khi nội dung quá dài (String or binary data would be truncated).
        // Ở đây cho các cột string "không phải khóa chính / không phải khóa ngoại (tên không kết thúc bằng Id) và chưa chỉ định độ dài rõ ràng" dùng nvarchar(max),
        // Cột khóa chính và khóa ngoại giữ độ dài mặc định, đảm bảo có thể tạo chỉ mục và hiệu quả truy vấn liên kết không bị ảnh hưởng.
        if (dataType == DataType.SqlServer) {
            fsql.Aop.ConfigEntityProperty += (_, e) => {
                if (e.Property.PropertyType != typeof(string)) return;
                if (e.ModifyResult.IsPrimary) return;
                if (e.Property.Name.EndsWith("Id", StringComparison.Ordinal)) return;
                if (e.ModifyResult.StringLength != 0) return; // Đã chỉ định độ dài rõ ràng (bao gồm -1)
                e.ModifyResult.StringLength = -1;
            };
        }

        return fsql;
    }

    /// <summary>
    /// Mặc định dùng SQL Server
    /// </summary>
    public static IFreeSql Create(string connectionString) {
        return CreateSqlServer(connectionString);
    }

    public static IFreeSql CreateSqlServer(string connectionString) {
        return Create(DataType.SqlServer, connectionString);
    }

    public static IFreeSql CreateMySql(string connectionString) {
        return Create(DataType.MySql, connectionString);
    }

    public static IFreeSql CreatePostgresSql(string connectionString) {
        return Create(DataType.PostgreSQL, connectionString);
    }
}