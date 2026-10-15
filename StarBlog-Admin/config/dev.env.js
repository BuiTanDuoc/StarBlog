'use strict'
const merge = require('webpack-merge')
const prodEnv = require('./prod.env')

/**
 * Cấu hình môi trường phát triển
 * Đọc thông tin cấu hình nhạy cảm từ biến môi trường, tránh mã hóa cứng vào mã nguồn
 */
module.exports = merge(prodEnv, {
  NODE_ENV: '"development"',
  BASE_URL: `"http://localhost:5038"`,
  EXCEPTION_LESS_API_KEY: `"${process.env.EXCEPTION_LESS_API_KEY || ''}"`,
  EXCEPTION_LESS_SERVER_URL: `"${process.env.EXCEPTION_LESS_SERVER_URL || 'http://localhost:5000'}"`,
})
