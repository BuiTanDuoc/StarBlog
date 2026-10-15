# Hướng dẫn cấu hình biến môi trường

## Tổng quan

Để tăng cường bảo mật, tránh việc mã hóa cứng các thông tin cấu hình nhạy cảm (như khóa API, địa chỉ máy chủ...) vào mã nguồn, dự án này đã chuyển sang cấu hình bằng biến môi trường.

## Các bước cấu hình

1. Sao chép tệp `.env.example` và đổi tên thành `.env`
2. Điền giá trị cấu hình thực tế vào tệp `.env`

## Giải thích biến môi trường

| Tên biến | Mô tả | Giá trị mặc định |
|--------|------|--------|
| `BASE_URL` | URL cơ sở của API backend | Môi trường phát triển: `http://localhost:5038`<br>Môi trường production: Trống |
| `EXCEPTION_LESS_API_KEY` | Khóa API Exceptionless | Trống |
| `EXCEPTION_LESS_SERVER_URL` | URL máy chủ Exceptionless | Môi trường phát triển: `http://localhost:5000`<br>Môi trường production: Trống |

## Cách sử dụng

### Môi trường phát triển

Tạo tệp `.env` trong thư mục gốc của dự án:

```bash
BASE_URL=http://localhost:5038
EXCEPTION_LESS_API_KEY=your_development_api_key
EXCEPTION_LESS_SERVER_URL=http://localhost:5000
```

### Môi trường production

Khi triển khai, hãy đặt biến môi trường hoặc tạo tệp `.env`:

```bash
BASE_URL=https://your-api-domain.com
EXCEPTION_LESS_API_KEY=your_production_api_key
EXCEPTION_LESS_SERVER_URL=https://your-exceptionless-server.com
```

## Lưu ý

1. Tệp `.env` đã được thêm vào `.gitignore`, sẽ không bị commit lên hệ thống quản lý phiên bản
2. Không mã hóa cứng thông tin nhạy cảm trong mã nguồn
3. Khi làm việc nhóm, hãy chia sẻ giá trị thực của biến môi trường qua kênh an toàn
4. Môi trường production nên dùng giải pháp quản lý khóa bí mật an toàn hơn