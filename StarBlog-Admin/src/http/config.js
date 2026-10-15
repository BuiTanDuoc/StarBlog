import {baseUrl} from '@/utils/global'

export default {
  method: 'get',
  // Tiền tố url cơ sở
  baseUrl: `${baseUrl}/Api`,
  // Thông tin header của yêu cầu
  headers: {
    'Content-Type': 'application/json;charset=UTF-8'
  },
  // Tham số
  data: {},
  // Đặt thời gian chờ (timeout)
  timeout: 990000,
  // Mang theo thông tin xác thực
  withCredentials: true,
  // Kiểu dữ liệu trả về
  responseType: 'json'
}
