using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarBlog.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visit_record",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ip = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ip_info_region_code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ip_info_country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ip_info_province = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ip_info_city = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ip_info_isp = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    request_path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    request_query_string = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    request_method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    user_agent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    user_agent_info_os_family = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent_info_os_major = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_agent_info_os_minor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_agent_info_os_patch = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_agent_info_os_patch_minor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_agent_info_device_brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent_info_device_family = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent_info_device_model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent_info_device_is_spider = table.Column<bool>(type: "bit", nullable: false),
                    user_agent_info_user_agent_family = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    user_agent_info_user_agent_major = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_agent_info_user_agent_minor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    user_agent_info_user_agent_patch = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status_code = table.Column<int>(type: "int", nullable: false),
                    response_time_ms = table.Column<int>(type: "int", nullable: false),
                    referrer = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visit_record", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_visit_status",
                table: "visit_record",
                column: "status_code");

            migrationBuilder.CreateIndex(
                name: "idx_visit_time",
                table: "visit_record",
                column: "time");

            migrationBuilder.CreateIndex(
                name: "idx_visit_time_status",
                table: "visit_record",
                columns: new[] { "time", "status_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visit_record");
        }
    }
}
