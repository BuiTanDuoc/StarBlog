import axios from 'axios';
import config from './config';
import Cookies from "js-cookie";
import router from '@/router'
import * as auth from '@/utils/auth'

export default function $axios(options) {
  return new Promise((resolve, reject) => {
    const instance = axios.create({
      baseURL: config.baseUrl,
      headers: config.headers,
      timeout: config.timeout,
      withCredentials: config.withCredentials
    })
    // request Bộ chặn yêu cầu
    instance.interceptors.request.use(
      config => {
        if (!auth.isLogin()) {
          // Chuyển hướng đến trang đăng nhập
          router.push('/login')
        }

        // Đính kèm khi gửi yêu cầu token
        let token = auth.getToken()
        config.headers.Authorization = `Bearer ${token}`

        return config
      },
      error => {
        // Khi yêu cầu xảy ra lỗi
        console.log('request:', error)
        // Kiểm tra yêu cầu hết thời gian chờ
        if (error.code === 'ECONNABORTED' && error.message.indexOf('timeout') !== -1) {
          console.log('timeout Yêu cầu hết thời gian chờ')
        }
        // Cần chuyển hướng đến trang lỗi
        const errorInfo = error.response
        console.log(errorInfo)
        if (errorInfo) {
          error = errorInfo.data  // Phía trang khi catch sẽ lấy được thông tin lỗi chi tiết, xem Promise.reject ở dưới cùng
          const errorStatus = errorInfo.status; // 404 403 500 ...
          router.push({
            path: `/error/${errorStatus}`
          })
        }
        return Promise.reject(error) // Phía nơi gọi có thể lấy được (catch) thông tin lỗi bạn muốn trả về
      }
    )

    // response Bộ chặn phản hồi
    instance.interceptors.response.use(
      response => {
        return response.data
      },
      err => {
        let reason = err
        if (err && err.response) {
          // Bọc lại thông báo lỗi mặc định
          switch (err.response.status) {
            case 400:
              err.message = 'Tham số yêu cầu không hợp lệ'
              break
            case 401:
              err.message = 'Vui lòng đăng nhập trước khi truy cập'
              break
            case 403:
              err.message = 'Từ chối truy cập, không có quyền truy cập'
              break
            case 404:
              err.message = `Địa chỉ yêu cầu bị lỗi: ${err.response.config.url}`
              break
            case 408:
              err.message = 'Yêu cầu hết thời gian chờ'
              break
            case 500:
              err.message = 'Lỗi nội bộ máy chủ'
              break
            case 501:
              err.message = 'Dịch vụ chưa được triển khai'
              break
            case 502:
              err.message = 'Lỗi gateway'
              break
            case 503:
              err.message = 'Dịch vụ không khả dụng'
              break
            case 504:
              err.message = 'Hết thời gian chờ của gateway'
              break
            case 505:
              err.message = 'Phiên bản HTTP không được hỗ trợ'
              break
            default:
          }

          // Nếu có phản hồi lỗi thì xử lý
          if (err.response.data) {
            reason = err.response.data
            // Nếu không có thông tin lỗi trả về thì dùng thông tin mặc định ở trên
            if (!reason.message) reason.message = err.message
          }
        }
        console.error(err)
        return Promise.reject(reason) // Trả về thông tin lỗi mà API trả về
      }
    )
    // Xử lý yêu cầu
    instance(options).then(res => {
      resolve(res)
      return false
    }).catch(error => {
      reject(error)
    })
  })
}
