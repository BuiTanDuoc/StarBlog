import Cookies from "js-cookie";

/**
 * Kiểm tra xem đã đăng nhập chưa
 * @return boolean
 */
export const isLogin = () => {
  let token = Cookies.get('token')
  let userName = localStorage.getItem('user')

  if (!token || token.length === 0) return false
  if (!userName || userName.length === 0) return false
  return !isExpired();
}

/**
 * Kiểm tra đăng nhập đã hết hạn chưa
 * @return boolean
 */
export const isExpired = () => {
  let expiration = localStorage.getItem('expiration')
  if (expiration) {
    let now = new Date()
    let expirationTime = new Date(expiration)
    if (now > expirationTime) return true
  }

  return false
}

/**
 * Đọc token đã lưu
 * @return string
 */
export const getToken = () => {
  return Cookies.get('token')
}

/**
 * Lưu thông tin đăng nhập
 * @param token Khóa bí mật
 * @param username Tên người dùng
 * @param expiration token Thời gian hết hạn
 */
export const login = (token, username, expiration) => {
  // Lưu token vào Cookie
  Cookies.set('token', token)
  // Lưu dữ liệu đăng nhập vào phiên cục bộ
  localStorage.setItem('user', username)
  localStorage.setItem('expiration', expiration)
}

// Đăng xuất
export const logout = () => {
  Cookies.set('token', null)
  localStorage.removeItem('user')
  localStorage.removeItem('expiration')
}

