using Microsoft.OpenApi;
using StarBlog.Web.Models;
using StarBlog.Web.Middlewares;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace StarBlog.Web.Extensions;

public static class ApiGroups {
    public const string Admin = "admin";
    public const string Auth = "auth";
    public const string Blog = "blog";
    public const string Comment = "comment";
    public const string Common = "common";
    public const string Link = "link";
    public const string Photo = "photo";
    public const string Test = "test";
}

public static class ConfigureSwagger {
    public static readonly List<SwaggerGroup> Groups = new() {
        new SwaggerGroup(ApiGroups.Admin, "Admin APIs", "Các API liên quan đến quản trị viên"),
        new SwaggerGroup(ApiGroups.Auth, "Auth APIs", "API xác thực"),
        new SwaggerGroup(ApiGroups.Blog, "Blog APIs", "API quản lý blog"),
        new SwaggerGroup(ApiGroups.Comment, "Comment APIs", "API bình luận"),
        new SwaggerGroup(ApiGroups.Common, "Common APIs", "API công khai dùng chung"),
        new SwaggerGroup(ApiGroups.Link, "Link APIs", "API liên kết bạn bè"),
        new SwaggerGroup(ApiGroups.Photo, "Photo APIs", "API quản lý hình ảnh"),
        new SwaggerGroup(ApiGroups.Test, "Test APIs", "API thử nghiệm")
    };

    public static void AddSwagger(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            Groups.ForEach(group => options.SwaggerDoc(group.Name, group.ToOpenApiInfo()));

            // Bật biểu tượng ổ khóa xanh
            var security = new OpenApiSecurityScheme {
                Description = "Xác thực kiểu JWT, vui lòng nhập \"Bearer {Token}\" để xác thực danh tính",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            options.AddSecurityDefinition("oauth2", security);
            options.AddSecurityRequirement(doc => {
                var securityRef = new OpenApiSecuritySchemeReference("oauth2", doc, string.Empty);
                return new OpenApiSecurityRequirement { { securityRef, new List<string>() } };
            });
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            // Chú thích XML
            var filePath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(filePath, true);
        });
    }

    /// <summary>
    /// Cấu hình middleware Swagger
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="requireAuth">Có cần xác thực để truy cập hay không, mặc định là true</param>
    public static void UseSwaggerPkg(this IApplicationBuilder app, bool requireAuth = true) {
        // Nếu cần xác thực thì thêm middleware xác thực Swagger
        if (requireAuth) {
            app.UseSwaggerAuth();
        }
        
        app.UseSwagger();
        app.UseSwaggerUI(opt => {
            opt.RoutePrefix = "api-docs/swagger";
            // Độ sâu mở rộng mặc định của model, đặt -1 để ẩn hoàn toàn model
            opt.DefaultModelsExpandDepth(-1);
            // Tài liệu API chỉ mở rộng tag
            opt.DocExpansion(DocExpansion.List);
            opt.DocumentTitle = "StarBlog APIs";
            // Nhóm
            Groups.ForEach(group => opt.SwaggerEndpoint($"/swagger/{group.Name}/swagger.json", group.Name));
        });
    }
}
