# StarBlog

![](./docs/images/logo.jpg)


<p align="center">
  <a href="https://dotnet.microsoft.com/download/dotnet/6.0"><img src="https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square&logo=dotnet" alt=".NET 6"></a>
  <a href="https://github.com/Deali-Axy/StarBlog-Admin"><img src="https://img.shields.io/badge/Admin-Vue3-4FC08D?style=flat-square&logo=vue.js" alt="Vue 3"></a>
  <a href="https://blog.deali.cn"><img src="https://img.shields.io/badge/Demo-Online-brightgreen?style=flat-square" alt="Online Demo"></a>
  <a href="https://github.com/Deali-Axy/StarBlog-Admin"><img src="https://img.shields.io/badge/Admin-Repository-181717?style=flat-square&logo=github" alt="Admin Repository"></a>
</p>

## 📝 Giới thiệu

StarBlog là một hệ thống blog hiện đại được phát triển dựa trên .NET 6 và ASP.NET Core, hỗ trợ nhập bài viết Markdown và tuân theo chuẩn RESTful API. Phần giao diện được phát triển bằng Vue + ElementUI, có thể dùng làm dự án học tập nhập môn .NET Core; đồng thời đi kèm một loạt ghi chép phát triển, ghi lại toàn bộ quá trình xây dựng hệ thống blog này từ con số không, giúp bạn học và hiểu quy trình phát triển một dự án .Net Core.

**Demo trực tuyến**: [https://blog.deali.cn](https://blog.deali.cn)

**Dự án trang quản trị**: [https://github.com/Deali-Axy/StarBlog-Admin](https://github.com/Deali-Axy/StarBlog-Admin)

**Công cụ đăng bài AI**: [https://github.com/star-blog/starblog-publisher](https://github.com/star-blog/starblog-publisher)

## 📫 Động lực thực hiện dự án

Có rất nhiều nền tảng blog trực tuyến, **vậy tại sao lại tự phát triển blog?**

Tác giả đã loay hoay với blog khá lâu rồi: từ lúc đầu dùng WordPress, rồi sau đó dùng hexo, hugo và các trình tạo blog tĩnh khác để đưa lên github hosting, lúc nào cũng loay hoay.

Loay hoay là để giải quyết vấn đề tốt hơn, nhưng cuối cùng vẫn quyết định tự bỏ thời gian làm một cái; ý tưởng cụ thể có thể xem tại [Ghi chép phát triển](https://www.cnblogs.com/deali/p/16104454.html)

- Không tìm được một blog trực tuyến nào khiến mình hài lòng
- Trải nghiệm viết blog trực tuyến không bằng viết cục bộ bằng typora
- Nếu viết bài cho tài khoản công chúng (WeChat) thì sự chú ý sẽ bị phân tán một phần vào việc làm sao viết cho thu hút người đọc chứ không phải bản thân bài viết
- Chỉ website của chính mình mới có toàn quyền kiểm soát, không bị nền tảng rác chèn thêm thông tin xấu làm ô nhiễm

## ✨ Tính năng

- **Hỗ trợ Markdown**: (**Tính năng cốt lõi💡**) Nhập hàng loạt bài viết Markdown cục bộ, hỗ trợ phân tích danh mục lồng nhiều cấp, tự động nhập hình ảnh và chuyển đổi nội dung
- **Quản lý bài viết**: Hỗ trợ đóng gói tải lên từng bài viết (kèm tệp hình ảnh đính kèm) và tự động nhập
- **Tác phẩm nhiếp ảnh**: Hỗ trợ tải lên hàng loạt tác phẩm nhiếp ảnh cục bộ, tự động đọc thông tin EXIF
- **Trang chủ tùy chỉnh**: Trang chủ blog có thể cấu hình, hỗ trợ biểu đồ/hiển thị ảnh ngẫu nhiên ở trang chủ, cấu hình bài viết/hình ảnh/danh mục được ghim
- **Chuyển đổi giao diện**: Giao diện blog phía người dùng hỗ trợ chuyển đổi nhiều phong cách chủ đề
- **Liên kết bạn bè**: Chức năng quản lý liên kết bạn bè đầy đủ, hỗ trợ người dùng tự gửi yêu cầu trao đổi liên kết
- **Thống kê truy cập**: Ghi nhận truy cập, thống kê và hiển thị trực quan hóa dữ liệu hiệu năng cao
- **API ảnh ngẫu nhiên**: Cung cấp một bộ API ảnh ngẫu nhiên, ảnh bìa bài viết mặc định dùng ảnh ngẫu nhiên
- **Hệ thống bình luận**: Tích hợp sẵn chức năng bình luận cho người dùng/khách, cùng chức năng tự động lọc và kiểm duyệt bình luận
- **Đăng ký RSS**: Hỗ trợ đăng ký RSS

## 🌟 Hệ sinh thái StarBlog

StarBlog không chỉ là một hệ thống blog, nó đang phát triển thành một hệ sinh thái blog hoàn chỉnh, bao gồm nhiều công cụ và dự án đi kèm:

### StarBlog Publisher

[StarBlog Publisher](https://github.com/star-blog/starblog-publisher) là một công cụ đăng bài chuyên nghiệp được thiết kế riêng cho hệ thống blog StarBlog, cung cấp cách đăng bài thuận tiện hơn so với việc đóng gói và tải lên truyền thống.

- **Hỗ trợ Markdown**: Hỗ trợ xem trước và đăng bài viết định dạng Markdown
- **Giao diện trực quan**: Cung cấp giao diện thân thiện với người dùng, dễ dàng quản lý và đăng nội dung blog
- **Hỗ trợ AI**: Tích hợp nhiều mô hình AI lớn hàng đầu (gồm ChatGPT của OpenAI, Claude của Anthropic và DeepSeek, v.v.)
- **Đa nền tảng**: Xây dựng bằng C# và .NET 8.0, chạy được trên Windows, macOS và Linux

### StarBlog Admin

[StarBlog Admin](https://github.com/Deali-Axy/StarBlog-Admin) là dự án trang quản trị của StarBlog, được phát triển bằng Vue + ElementUI, cung cấp chức năng quản lý nội dung blog đầy đủ.

### StarBlogHub

Xây dựng một nền tảng tổng hợp blog phi tập trung, các blog cá nhân khác nhau đều có thể kết nối vào và chia sẻ lưu lượng truy cập.

## 🏗️ Kiến trúc

### Công nghệ sử dụng

#### Backend

##### Framework cốt lõi và hạ tầng
- **Web Framework**: ASP.NET Core - Framework ứng dụng Web .NET đa nền tảng, hiệu năng cao
- **Giám sát và gỡ lỗi**: Rin - Công cụ kiểm tra thời gian thực cho ứng dụng ASP.NET Core

##### Truy cập và xử lý dữ liệu
- **ORM chính**: FreeSql - Framework ORM hiệu năng cao, hỗ trợ nhiều loại cơ sở dữ liệu
- **ORM phụ**: Entity Framework Core - Framework ORM chính thức của Microsoft
- **Ánh xạ đối tượng**: AutoMapper - Thư viện công cụ giúp đơn giản hóa ánh xạ đối tượng - đối tượng

##### API và xác thực
- **Tài liệu API**: Swagger/OpenAPI (Swashbuckle.AspNetCore) - Công cụ tự động sinh tài liệu RESTful API
- **Cơ chế xác thực**: JWT (JSON Web Token) - Giải pháp xác thực danh tính xuyên miền an toàn
- **Hỗ trợ công cụ tìm kiếm**: RobotsTxtCore - Quản lý chính sách truy cập của trình thu thập dữ liệu công cụ tìm kiếm

##### Xử lý nội dung
- **Engine Markdown**: [Markdig](https://github.com/xoofx/markdig) - Bộ xử lý Markdown hiệu năng cao
- **Xử lý hình ảnh**: ImageSharp - Thư viện xử lý hình ảnh đa nền tảng
- **Thành phần phân trang**: X.PagedList - Giải pháp phân trang dữ liệu hiệu quả
- **Hỗ trợ RSS**: System.ServiceModel.Syndication - Công cụ tổng hợp nội dung RSS

##### Giao tiếp và công cụ
- **Dịch vụ email**: MailKit - Thư viện email client đa nền tảng

#### Frontend
- **Giao diện blog**: Bootstrap + Vue + ElementUI + editor.md + bootswatch
- **Trang quản trị**: Vue + Vuex + Vue Router + ElementUI + SCSS

### Cấu trúc dự án

```
StarBlog/
├── src/
│   ├── StarBlog.Api/       # WebAPI thuần (tầng HTTP / composition root: Controllers + Middlewares + DI)
│   ├── StarBlog.Application/ # Tầng ứng dụng (điều phối use case / application service / DTO / tác vụ nền, v.v., phục vụ StarBlog.Api)
│   ├── StarBlog.Infrastructure/ # Hạ tầng dùng chung (chẩn đoán / các tiện ích khác)
│   ├── StarBlog.Content/   # Xử lý nội dung (Markdown/ToC/xử lý nhập, v.v.)
│   ├── StarBlog.Data/      # Mô hình dữ liệu và tầng truy cập dữ liệu (EF Core + FreeSql)
│   └── StarBlog.Web/       # Ứng dụng Web chính (MVC/Razor + API, đường dẫn lịch sử, hiện không thay đổi)
├── tools/
│   ├── MarkdownImporter/   # Công cụ CLI nhập bài viết blog (trước đây là StarBlog.Migrate)
│   ├── DataProc/           # Công cụ xử lý dữ liệu (bổ sung nhật ký truy cập / tối ưu hình ảnh / tóm tắt, v.v.)
│   └── BlogImageOptimizer/ # Công cụ tối ưu hình ảnh
└── demo/                   # Dự án demo/thử nghiệm (tách biệt khỏi sản phẩm chính)
```

### Phân tầng và trách nhiệm (hướng DDD/Clean Architecture)

Cách phân chia này gần với hướng “tư tưởng phân tầng của DDD / Clean Architecture” hơn: tách tầng thích ứng HTTP khỏi “điều phối use case (Application Layer)”, giúp phát triển logic nghiệp vụ và hạ tầng mà không ảnh hưởng đến API công khai.

#### Phân tầng hiện tại (đã triển khai)

```
StarBlog.Api (Presentation / Delivery)
  └─ phụ thuộc → StarBlog.Application (Application Layer)
             └─ phụ thuộc → StarBlog.Data / StarBlog.Infrastructure / StarBlog.Content
```

- **StarBlog.Api**
  - Chỉ phụ trách HTTP: Controllers, middleware xác thực/Swagger/HealthChecks, DI composition root, định tuyến và chính sách CORS, v.v.
  - Không chứa quy tắc nghiệp vụ và điều phối phức tạp, giữ “controller mỏng”.
- **StarBlog.Application**
  - Chứa tầng ứng dụng: application service (điều phối Use Case), DTO/ViewModels, đối tượng tham số truy vấn, tác vụ nền (Outbox/VisitRecord Worker, v.v.).
  - Mục tiêu là “hành vi nghiệp vụ ổn định + dễ kiểm thử”, đồng thời tránh phụ thuộc vào StarBlog.Api (hướng phụ thuộc một chiều).

#### Đây có phải DDD không?

- **Đúng hướng “tư tưởng phân tầng” của DDD**: `StarBlog.Application` hiện tại về cơ bản tương ứng với Application Layer của DDD; `StarBlog.Api` tương ứng với Presentation/Interface Adapter.
- **Nhưng chưa phải “DDD đầy đủ”** (hiện trạng là “kiến trúc phân tầng + domain chưa được thể hiện tường minh”):
  - Chưa có module `StarBlog.Domain` độc lập để biểu đạt aggregate/value object/domain service/domain event, v.v.
  - `StarBlog.Data.Models` vẫn đảm nhận phần lớn mô hình domain (thiên về “mô hình lưu trữ / mô hình thiếu hành vi (anemic)”), phần trừu tượng hóa repository cũng chưa tách hẳn khỏi hạ tầng.

#### Lộ trình phát triển đề xuất (tùy chọn)

Nếu sau này muốn tiến gần hơn tới DDD (hoặc cách phân tầng kinh điển của Clean Architecture), có thể dần dần đưa vào:

```
StarBlog.Api → StarBlog.Application → StarBlog.Domain
                                 ↘  (interface) ↙
                       StarBlog.Infrastructure / StarBlog.Data (triển khai)
```

Nguyên tắc cốt lõi: phụ thuộc hướng vào trong (tầng có quy tắc nghiệp vụ ổn định hơn không phụ thuộc vào tầng dễ thay đổi hơn), HTTP/cơ sở dữ liệu/dịch vụ bên thứ ba đều được triển khai và thay thế ở tầng biên.

## 🚀 Bắt đầu nhanh

### Yêu cầu môi trường

- **.NET 9 SDK**
- **Node.js v18** trở lên và **npm/yarn** (dùng để quản lý tài nguyên frontend)

### Các bước build

1. **Clone repository**

```bash
git clone https://github.com/Deali-Axy/StarBlog.git
cd StarBlog
```

2. **Chuẩn bị tài nguyên frontend**

Dự án này dùng NPM + Gulp để quản lý các tệp tĩnh frontend, cần Nodejs 18 trở lên, chi tiết xem tại: [Ghi chép phát triển AspNetCore: Dùng NPM và gulp để quản lý tệp tĩnh frontend](https://www.cnblogs.com/deali/p/15905760.html).

```bash
cd src/StarBlog.Web
npm i -g bower
npm install  # hoặc yarn
npm install --global gulp-cli
gulp move
gulp min
```

**Lưu ý**: Dự án này phụ thuộc vào thành phần [bootstrap5-treeview](https://www.npmjs.com/package/bootstrap5-treeview). Thành phần này lại dùng bower để build, vì vậy hãy cài [bower](https://bower.io/) trước: `npm i -g bower`, nếu không sẽ báo lỗi trong quá trình chạy `npm install`.

3. **Chạy dự án**

Mở solution bằng Visual Studio hoặc Rider, đặt `StarBlog.Web` làm dự án khởi động rồi chạy.

Giải thích về solution:
- `StarBlog.sln`: Dự án sản phẩm chính (src)
- `StarBlog.Tools.sln`: Dự án công cụ (tools)
- `StarBlog.Demo.sln`: Dự án demo/thử nghiệm (demo)

Dự án này sử dụng cơ sở dữ liệu **SQL Server (MSSQL)**, hầu hết các chức năng đều dùng FreeSQL làm ORM; chỉ cần tạo trước một cơ sở dữ liệu trống (ví dụ `StarBlog`), sau đó sửa chuỗi kết nối trong `appsettings.json`, khi chạy dự án FreeSQL sẽ tự động sinh cấu trúc bảng:

```json
"ConnectionStrings": {
  "SqlServer": "Server=localhost;Database=StarBlog;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;",
  "SqlServer-Log": "Server=localhost;Database=StarBlog;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
}
```

- `SqlServer`: Cơ sở dữ liệu nghiệp vụ (FreeSQL)
- `SqlServer-Log`: Cơ sở dữ liệu ghi nhận truy cập/thống kê (EF Core), có thể dùng chung một cơ sở dữ liệu với cơ sở dữ liệu nghiệp vụ, cũng có thể tách riêng

4. **Đồng bộ cơ sở dữ liệu nhật ký truy cập**

Module nhật ký truy cập của StarBlog để tối ưu hiệu năng nên dùng một cơ sở dữ liệu độc lập, được quản lý bằng EFCore, xem chi tiết: [StarBlog Ngoại truyện (1) Chức năng thống kê truy cập hoàn toàn mới, hàng đợi bất đồng bộ, lưu trữ tách cơ sở dữ liệu](https://blog.deali.cn/Blog/Post/a97ecc01df52707a)

EFCore không thể tự động sinh cấu trúc bảng như FreeSQL, cần đồng bộ cơ sở dữ liệu thủ công, cũng dùng SQL Server. Các migration phiên bản SQLite vốn có trong repository đã bị gỡ bỏ, lần đầu sử dụng hãy tạo migration khởi tạo cho SQL Server (`dotnet ef migrations add InitialCreate`).

Trước tiên cài đặt công cụ cli của EFCore:

```bash
dotnet tool install --global dotnet-ef
```

Đồng bộ cơ sở dữ liệu (Windows10+)

```powershell
cd src/StarBlog.Data
$env:CONNECTION_STRING = "Server=localhost;Database=StarBlog;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
# Lần đầu: tạo migration khởi tạo cho SQL Server
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Đồng bộ cơ sở dữ liệu (Linux/MacOS)

```bash
cd src/StarBlog.Data
export CONNECTION_STRING="Server=localhost;Database=StarBlog;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
# Lần đầu: tạo migration khởi tạo cho SQL Server
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Khởi tạo
**Cách nhanh (seed sẵn tài khoản quản trị):** khi khởi động, nếu bảng `user` đang rỗng thì ứng dụng tự tạo tài khoản theo mục `Seed:Admin` trong `appsettings.json` (mặc định `admin` / `Admin@123456`) và đánh dấu đã khởi tạo, nên không cần vào `/Home/Init`. **Hãy đổi mật khẩu mặc định trước khi triển khai thật**, hoặc đặt `"Enabled": false` để tắt seed.

```json
"Seed": {
  "Admin": { "Enabled": true, "Username": "admin", "Password": "Admin@123456" }
}
```

**Cách thủ công:**

Sau khi khởi động dự án StarBlog lần đầu, truy cập `/Home/Init` để thực hiện các thao tác khởi tạo như tạo tài khoản quản trị viên, sau đó mới có thể dùng tài khoản quản trị này để đăng nhập trang quản trị.

**Lưu ý**: Thao tác khởi tạo chỉ có thể thực hiện một lần. Chi tiết xem [StarBlog - (16) Một số tính năng mới (giám sát/thống kê/cấu hình/khởi tạo)](https://www.cnblogs.com/deali/p/16523157.html).

### Cấu hình

#### Cấu hình email

Liên kết bạn bè và hệ thống bình luận của StarBlog đều dùng chức năng gửi email, chi tiết xem: [StarBlog - (28) Phát triển các API liên quan đến liên kết bạn bè](https://blog.deali.cn/Blog/Post/f55c8c7610706503)

Vui lòng thêm tệp `appsettings.email.json` vào thư mục gốc của dự án, lấy email Outlook làm ví dụ, cấu hình như sau

```json
"EmailAccountConfig": {
  "Host": "smtp-mail.outlook.com",
  "Port": 587,
  "FromUsername": "dia-chi-email@outlook.com",
  "FromPassword": "mat-khau-email",
  "FromAddress": "dia-chi-email@outlook.com"
}
```

#### Phát hiện từ nhạy cảm

StarBlog sử dụng kỹ thuật DFA để phát hiện từ nhạy cảm trong bình luận; khi sử dụng cần đặt một tệp thư viện từ nhạy cảm `words.json` trong dự án StarBlog.Web

Để giữ môi trường mạng văn minh và hài hòa, mã nguồn mở của dự án này không thể cung cấp sẵn, bạn nào cần có thể tự thu thập.

Định dạng như sau

```json
[
  {
    "Id": 1,
    "Value": "dễ thương",
    "Tag": "Bạo lực"
  },
  {
    "Id": 2,
    "Value": "cua sông",
    "Tag": "Quảng cáo"
  }
]
```

>Nếu thực sự cần cho mục đích học tập, thử nghiệm, bạn có thể nhắn tin riêng từ khóa badwords ở hậu trường tài khoản công chúng (WeChat) "Phòng thí nghiệm lập trình" để nhận.
>
>**Tuyên bố miễn trừ trách nhiệm (Sensitive Word List Disclaimer)**
>
>Thư viện từ nhạy cảm này chỉ phục vụ mục đích học tập, nghiên cứu và thử nghiệm. **Nghiêm cấm sử dụng thư viện từ nhạy cảm này cho bất kỳ mục đích thương mại, mục đích bất hợp pháp hoặc các hành vi khác có thể vi phạm pháp luật tại nơi bạn sinh sống.**
>
>Xin lưu ý:
>
>- Nội dung thư viện từ nhạy cảm có nguồn gốc rộng rãi, **không đại diện cho bất kỳ lập trường hay quan điểm nào của dự án StarBlog hoặc tác giả**.
>- **Nghiêm cấm người dùng phát tán, chia sẻ, bán lại, công bố công khai toàn bộ hoặc một phần nội dung thư viện từ nhạy cảm này dưới bất kỳ hình thức nào**.
>- Trong quá trình sử dụng, người dùng phải tuân thủ pháp luật hiện hành, **mọi trách nhiệm pháp lý phát sinh do đó do người sử dụng tự chịu**.
>- Vì lý do tuân thủ và an toàn, nội dung thư viện từ đã được mã hóa, người sử dụng tự chịu rủi ro khi giải mã và sử dụng.
>
>**Việc sử dụng đồng nghĩa với việc bạn đã đọc và đồng ý toàn bộ nội dung của tuyên bố miễn trừ trách nhiệm này.**

## Triển khai

StarBlog hỗ trợ nhiều cách triển khai, cách có rào cản thấp nhất là publish ở chế độ self-container, sau đó chạy trực tiếp trên máy chủ.

```bash
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true  --self-contained true
```

Sau khi publish, tải các tệp trong thư mục `publish` lên máy chủ, chạy tệp **StarBlog.Web** là được.

Các cách triển khai khác có thể tham khảo: [StarBlog - (31) Publish và triển khai](https://blog.deali.cn/p/starblog-31)


## 📚 Ghi chép phát triển

Dự án này đi kèm một loạt ghi chép phát triển, ghi lại toàn bộ quá trình xây dựng hệ thống blog này từ con số không, đồng thời có thể dùng làm giáo trình học nhập môn phát triển .NetCore.

> PS: Mã nguồn dự án cập nhật khá nhanh, để học theo loạt bài ghi chép phát triển, vui lòng dùng [mã nguồn phiên bản v1.0.0](https://github.com/Deali-Axy/StarBlog/tree/v1.0.0)
>
> lệnh clone
>
> ```bash
> git clone --branch v1.0.0 --single-branch https://github.com/Deali-Axy/StarBlog.git
> ```

- [Phát triển dự án blog bằng .NetCore StarBlog - (1) Tại sao cần tự viết một blog?](https://www.cnblogs.com/deali/p/16104454.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (2) Chuẩn bị môi trường và tạo dự án](https://www.cnblogs.com/deali/p/16172342.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (3) Thiết kế mô hình](https://www.cnblogs.com/deali/p/16180920.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (4) Nhập hàng loạt blog markdown](https://www.cnblogs.com/deali/p/16211720.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (5) Bắt đầu dựng dự án Web](https://www.cnblogs.com/deali/p/16276448.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (6) Phát triển trang: danh sách bài viết blog](https://www.cnblogs.com/deali/p/16286780.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (7) Phát triển trang: trang chi tiết bài viết](https://www.cnblogs.com/deali/p/16293309.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (8) Hiển thị cấu trúc phân cấp danh mục](https://www.cnblogs.com/deali/p/16307604.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (9) Nhập hàng loạt hình ảnh](https://www.cnblogs.com/deali/p/16328825.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (10) Hình ảnh dạng thác nước (waterfall)](https://www.cnblogs.com/deali/p/16335162.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (11) Triển khai thống kê truy cập](https://www.cnblogs.com/deali/p/16349155.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (12) Biên dịch động trang Razor](https://www.cnblogs.com/deali/p/16391656.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (13) Thêm chức năng liên kết bạn bè](https://www.cnblogs.com/deali/p/16421699.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (14) Triển khai chức năng chuyển đổi giao diện](https://www.cnblogs.com/deali/p/16441294.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (15) Tạo hình ảnh kích thước ngẫu nhiên](https://www.cnblogs.com/deali/p/16457314.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (16) Một số tính năng mới (giám sát/thống kê/cấu hình/khởi tạo)](https://www.cnblogs.com/deali/p/16523157.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (17) Tự động tải xuống hình ảnh bên ngoài trong bài viết](https://www.cnblogs.com/deali/p/16586437.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (18) Triển khai đóng gói và tải lên bài viết Typora cục bộ](https://www.cnblogs.com/deali/p/16758878.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (19) Khám phá giải pháp render Markdown](https://www.cnblogs.com/deali/p/16834452.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (20) Tối ưu hiển thị hình ảnh](https://www.cnblogs.com/deali/p/16929677.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (21) Bắt đầu phát triển RESTFul API](https://www.cnblogs.com/deali/p/16989798.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (22) Phát triển các API liên quan đến bài viết blog](https://www.cnblogs.com/deali/p/16991279.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (23) Phân trang, lọc, tìm kiếm, sắp xếp cho API danh sách bài viết](https://www.cnblogs.com/deali/p/16992573.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (24) Thống nhất định dạng dữ liệu trả về của API](https://www.cnblogs.com/deali/p/16995384.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (25) API hình ảnh và tải tệp lên](https://www.cnblogs.com/deali/p/16999818.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (26) Tích hợp tài liệu API Swagger](https://www.cnblogs.com/deali/p/17093390.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (27) Dùng JWT để bảo vệ API](https://blog.sblt.deali.cn:9000/Blog/Post/541b8beae183d29e)
- [Phát triển dự án blog bằng .NetCore StarBlog - (28) Phát triển các API liên quan đến liên kết bạn bè](https://www.cnblogs.com/deali/p/starblog-28.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (29) Phát triển chức năng đăng ký RSS](https://www.cnblogs.com/deali/p/17501704.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (30) Triển khai hệ thống bình luận](https://www.cnblogs.com/deali/p/17910094.html)
- [Phát triển dự án blog bằng .NetCore StarBlog - (31) Publish và triển khai](https://www.cnblogs.com/deali/p/18011965)
- [Phát triển dự án blog bằng .NetCore StarBlog - (32) Kết thúc giai đoạn một](https://www.cnblogs.com/deali/p/18582026)
- ...

Ngoại truyện:

- [StarBlog Ngoại truyện (1) Chức năng thống kê truy cập hoàn toàn mới, hàng đợi bất đồng bộ, lưu trữ tách cơ sở dữ liệu](https://blog.deali.cn/Blog/Post/a97ecc01df52707a)
- [StarBlog Ngoại truyện (2) Phân tích sâu mã nguồn Markdig, tối ưu logic trích xuất tiêu đề ToC và sinh cây mục lục bài viết](https://blog.deali.cn/Blog/Post/d47403a6c7399c44)
- [StarBlog Ngoại truyện (3) StarBlog Publisher, đăng bài đa nền tảng chỉ một cú nhấp, công cụ sáng tác bài viết được DeepSeek hỗ trợ](https://blog.deali.cn/Blog/Post/211884ccc25b02e2)


## 📷 Ảnh chụp màn hình

### Trang chủ blog

![](./docs/images/home.png)

### Danh sách bài viết

![](./docs/images/post-list.jpg)

### Trang nhiếp ảnh ~~(tuy hiện giờ vẫn chưa đưa những tấm ảnh đã chụp lên)~~

![](./docs/images/photos.png)

### Trang chủ trang quản trị

![](./docs/images/admin.png)

### Danh sách bài viết trong trang quản trị

![](./docs/images/admin-posts.png)

### Giao diện chỉnh sửa bài viết

![](./docs/images/admin-post-edit.png)

### Danh sách hình ảnh trong trang quản trị

![](./docs/images/admin-photos.png)

## 🔗 Theo dõi tôi

Tài khoản công chúng (WeChat) | Mã QR tài khoản công chúng 
------- | ------ 
![](./docs/images/coding_lab_logo.jpg) | ![](./docs/images/coding_lab_qr_code.jpg)


## 🔄 Những điểm còn thiếu sót của phiên bản hiện tại

Từ khi dự án StarBlog ra mắt đến nay, tôi liên tục học thêm các kiến thức chi tiết về AspNetCore; so với lúc mới phát triển dự án này, mức độ quen thuộc với framework đã tăng lên đôi chút, tự nhiên cũng phát hiện ra những hạn chế trong mã nguồn trước đây:

- Thao tác “truy vấn” trong CRUD nên dùng phương thức patch
- Thêm `[HttpHead]` vào các API phương thức Get để hỗ trợ phương thức Head
- Các API lọc và tìm kiếm cần trim tham số
- Không nên đổi toàn bộ giá trị trả về của API thành kiểu `ApiResponse`, nên giữ kiểu `ActionResult` của framework để có nhiều chức năng hơn
- Chỉ thống nhất giá trị trả về của API mà chưa bọc ngoại lệ, nên dùng middleware `app.UseExceptionHandler` để xử lý lỗi thống nhất (cũng có thể dùng exception filter)
- Hỗ trợ markdown vẫn chưa tốt ở các phần toc, công thức, khối mã, bảng lồng hình ảnh, v.v.

Những vấn đề này sẽ được giải quyết ở phiên bản v2.

## 🚀 Kế hoạch phiên bản v2 mới

Hiện đã lên kế hoạch một số tính năng mới và tối ưu hóa, nhưng chắc chắn đây chưa phải toàn bộ của phiên bản v2, các bạn nếu có đề xuất hay cũng có thể để lại bình luận thảo luận nhé~

### Tái cấu trúc giao diện blog

- Tái cấu trúc bằng Next.js/Remix/Astro (công nghệ cụ thể sẽ quyết định sau)
- Dùng chức năng phân tích markdown của hệ sinh thái nodejs

### Tái cấu trúc trang quản trị

- Tái cấu trúc bằng công nghệ dựa trên react

### Chức năng thống kê truy cập mới

- Trực quan hóa thông tin địa lý
- Phân tích việc công cụ tìm kiếm lập chỉ mục
- Chức năng chống crawler
- Thống kê lượt đọc bài viết

### Chức năng chỉnh sửa bài viết

- Dùng trình soạn thảo markdown mới (tốt nhất là kiểu WYSIWYG như wagtail)
- Hỗ trợ thêm nhiều nội dung hơn vào bài viết (như video)

### Tối ưu trải nghiệm đọc bài viết

- Dùng công cụ render markdown mới (hiện đang dùng bản [editor.md](https://github.com/Deali-Axy/editor.md-ext) do tôi fork và chỉnh sửa, dùng cũng tạm được, nhưng công cụ này đã rất cũ và cũng ngừng cập nhật, tôi hy vọng tìm được một công cụ render hiện đại hơn, được bảo trì tốt để thay thế)

### Mã hóa bài viết

- Đặt mật khẩu cố định
- Theo dõi tài khoản công chúng để lấy mật khẩu động

### Chức năng tìm kiếm mới

- Dùng công cụ tìm kiếm toàn văn
- Thêm Embedding

### Chức năng AI

- Cơ sở tri thức
- Chức năng hội thoại
- Tóm tắt bài viết bằng AI
- Bình luận tự động



## 🙏 Lời cảm ơn

<p align="center">
  <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/Rider.png" alt="JetBrains Rider" width="200">
</p>

<p align="center">
  Cảm ơn <a href="https://jb.gg/OpenSourceSupport">JetBrains Open Source Support</a> đã cung cấp công cụ phát triển miễn phí Rider
</p>
