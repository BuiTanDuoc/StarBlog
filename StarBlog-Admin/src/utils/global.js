/**
 * Module đóng gói hằng số và phương thức toàn cục
 * Gắn vào thuộc tính Vue thông qua prototype
 * Gọi qua this.Global
 */

// Địa chỉ máy chủ hệ thống quản trị
export const baseUrl = process.env.BASE_URL

export default {
  baseUrl
}
