'use strict'

/**
 * Cấu hình môi trường production
 * Đọc thông tin cấu hình nhạy cảm từ biến môi trường, tránh mã hóa cứng vào mã nguồn
 */
module.exports = {
  NODE_ENV: '"production"',
  BASE_URL: `"${process.env.BASE_URL || ''}"`,
  EXCEPTION_LESS_API_KEY: `"${process.env.EXCEPTION_LESS_API_KEY || ''}"`,
  EXCEPTION_LESS_SERVER_URL: `"${process.env.EXCEPTION_LESS_SERVER_URL || ''}"`
}
