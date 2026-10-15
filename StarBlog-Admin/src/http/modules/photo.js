import axios from '../axios'

// Lấy danh sách
export const getList = (page = 1, pageSize = 10) => {
  return axios({
    url: 'Photo',
    method: 'get',
    params: {page, pageSize}
  })
}

// Lấy mục được chỉ định
export const get = photoId => {
  return axios({
    url: `Photo/${photoId}/`,
    method: 'get',
  })
}

// Tải ảnh lên
export const add = (title, location, file) => {
  let formData = new FormData()
  formData.append('title', title)
  formData.append('location', location)
  formData.append('file', file)

  return axios({
    url: 'Photo',
    method: 'post',
    headers: {'Content-Type': 'multipart/form-data;charset=UTF-8'},
    data: formData
  })
}

// Xóa mục được chỉ định
export const deleteItem = itemId => {
  return axios({
    url: `Photo/${itemId}/`,
    method: 'delete'
  })
}

// Đặt đề xuất
export const setFeatured = itemId => {
  return axios({
    url: `Photo/${itemId}/SetFeatured`,
    method: 'post'
  })
}

// Bỏ đề xuất
export const cancelFeatured = itemId => {
  return axios({
    url: `Photo/${itemId}/CancelFeatured`,
    method: 'post'
  })
}

// Cập nhật thông tin ảnh
export const update = data => {
  return axios({
    url: `Photo/${data.id}/`,
    method: 'put',
    data
  })
}
