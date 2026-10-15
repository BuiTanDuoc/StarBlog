# StarBlog Admin

> Một trang quản trị blog hiện đại, được xây dựng bằng Vue + ElementUI

[![Vue](https://img.shields.io/badge/Vue-2.5.2-brightgreen.svg)](https://vuejs.org/)
[![Element UI](https://img.shields.io/badge/Element_UI-2.15.6-blue.svg)](https://element.eleme.io/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Giới thiệu dự án

StarBlog Admin là trang quản trị của hệ thống blog [StarBlog](https://github.com/Deali-Axy/StarBlog), cung cấp đầy đủ chức năng quản lý nội dung blog. StarBlog là một hệ thống blog hiện đại được phát triển trên .NET 6 và ASP.NET Core, hỗ trợ nhập bài viết Markdown và tuân theo chuẩn giao diện RESTful.

Dự án này được phát triển dựa trên Vue 2 và Element UI, có thể dùng làm dự án học Vue cơ bản, đồng thời là ví dụ phần frontend của phát triển full-stack .NET Core.

## Tính năng nổi bật

- 📝 **Quản lý bài viết**: hỗ trợ tạo, chỉnh sửa, xóa bài viết blog, hỗ trợ trình soạn thảo Markdown
- 🏷️ **Quản lý danh mục**: hệ thống danh mục bài viết linh hoạt, hỗ trợ đặt danh mục đề xuất
- 📊 **Thống kê dữ liệu**: thống kê lượt truy cập, phân tích dữ liệu đọc bài viết
- 📷 **Tác phẩm nhiếp ảnh**: hỗ trợ tải lên và quản lý tác phẩm nhiếp ảnh
- 🔗 **Quản lý liên kết bạn bè**: hỗ trợ thêm và quản lý liên kết bạn bè
- 🎨 **Chuyển đổi chủ đề**: hỗ trợ chuyển đổi nhiều màu chủ đề
- 🔐 **Xác thực người dùng**: đăng nhập an toàn và quản lý quyền người dùng

## Công nghệ sử dụng

- **Framework frontend**: Vue 2.5.2
- **UI Thư viện thành phần**: Element UI 2.15.6
- **Quản lý trạng thái**: Vuex 3.6.2
- **Quản lý route**: Vue Router 3.0.1
- **HTTP Yêu cầu**: Axios
- **Markdown Trình soạn thảo**: v-md-editor
- **Trực quan hóa dữ liệu**:@jiaminghi/data-view
- **CSS Bộ tiền xử lý**: Sass

## Cài đặt và sử dụng

### Yêu cầu môi trường

- Node.js >= 6.0.0
- npm >= 3.0.0 hoặc pnpm

### Khuyến nghị sử dụng pnpm

```bash
# Cài đặt phụ thuộc
pnpm i

# Khởi động máy chủ phát triển, địa chỉ mặc định localhost:8080
pnpm dev

# Build phiên bản môi trường production
pnpm build
```

### Sử dụng npm

```bash
# Cài đặt phụ thuộc
npm install

# Khởi động máy chủ phát triển, địa chỉ mặc định localhost:8080
npm run dev

# Build phiên bản môi trường production
npm run build

# Build phiên bản môi trường production và xem báo cáo phân tích
npm run build --report

# Chạy kiểm thử đơn vị
npm run unit

# Chạy tất cả kiểm thử
npm test
```

## Cấu trúc dự án

```
├── build/            # Webpack Tệp cấu hình
├── config/           # Tệp cấu hình dự án
├── src/              # Mã nguồn
│   ├── assets/       # Tài nguyên tĩnh
│   ├── components/   # Thành phần dùng chung
│   ├── http/         # API Cấu hình API và HTTP
│   ├── router/       # Cấu hình route
│   ├── store/        # Vuex Quản lý trạng thái
│   ├── utils/        # Hàm tiện ích
│   ├── views/        # Component trang (view)
│   ├── App.vue       # Component gốc
│   └── main.js       # Tệp đầu vào
├── static/           # Tệp tĩnh
└── test/             # Tệp kiểm thử
```

## Liên kết liên quan

- Dự án backend:[StarBlog](https://github.com/Deali-Axy/StarBlog)
- Vue.js Tài liệu:[https://vuejs.org/](https://vuejs.org/)
- Element UI Tài liệu:[https://element.eleme.io/](https://element.eleme.io/)

## Giấy phép

[MIT License](LICENSE)
