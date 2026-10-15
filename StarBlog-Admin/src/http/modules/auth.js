import axios from '../axios'


// Đăng nhập
export const login = data => {
  return axios({
    url: 'Auth',
    method: 'post',
    data
  })
}

// Lấy thông tin người dùng
export const get = () => axios({
  url: 'Auth',
  method: 'get'
})
