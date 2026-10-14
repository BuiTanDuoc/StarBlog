using System.Diagnostics;
using FreeSql;
using FreeSql.Internal;

namespace StarBlog.Data;

public class FreeSqlFactory {
    public static IFreeSql Create(DataType dataType, string connectionString) {
        var fsql = new FreeSql.FreeSqlBuilder()
            .UseConnectionString(dataType, connectionString)
            .UseNameConvert(NameConvertType.PascalCaseToUnderscoreWithLower)
            .UseAutoSyncStructure(true) //自动同步实体结构到数据库
            .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
            .Build(); //请务必定义成 Singleton 单例模式

        // FreeSql 对未指定长度的 string 属性默认生成 nvarchar(255)。
        // SQLite 不校验长度所以一直没问题，但 SQL Server 会在内容超长时报错（String or binary data would be truncated）。
        // 这里让「非主键 / 非外键（名称不以 Id 结尾）且没显式指定长度」的 string 列使用 nvarchar(max)，
        // 主键与外键列保持默认长度，保证可以建索引、关联查询效率不受影响。
        if (dataType == DataType.SqlServer) {
            fsql.Aop.ConfigEntityProperty += (_, e) => {
                if (e.Property.PropertyType != typeof(string)) return;
                if (e.ModifyResult.IsPrimary) return;
                if (e.Property.Name.EndsWith("Id", StringComparison.Ordinal)) return;
                if (e.ModifyResult.StringLength != 0) return; // 已显式指定长度（包括 -1）
                e.ModifyResult.StringLength = -1;
            };
        }

        return fsql;
    }

    /// <summary>
    /// 默认使用 SQL Server
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