import axios from '../axios'

/**
 * Lấy danh sách blog
 * @param isPublish
 * @param status
 * @param categoryId
 * @param search
 * @param sortBy
 * @param page
 * @param pageSize
 * @returns {*}
 */
export const getList = (
  isPublish = false,
  status = '',
  categoryId = 0,
  search = '',
  sortBy = '',
  page = 1,
  pageSize = 20
) => {
  return axios({
    url: 'BlogPost',
    method: 'get',
    params: {isPublish, status, categoryId, search, sortBy, page, pageSize}
  })
}

// Lấy bài viết blog được chỉ định
export const get = itemId => {
  return axios({
    url: `BlogPost/${itemId}/`,
    method: 'get',
  })
}

// Xóa blog được chỉ định
export const deleteItem = itemId => {
  return axios({
    url: `BlogPost/${itemId}/`,
    method: 'delete'
  })
}

// Thêm blog
export const add = data => {
  return axios({
    url: `BlogPost`,
    method: 'post',
    data
  })
}

// Cập nhật bài viết blog
export const update = data => {
  return axios({
    url: `BlogPost/${data.id}/`,
    method: 'put',
    data
  })
}

/**
 * Tải ảnh lên
 * @param itemId ID bài viết
 * @param file Tệp ảnh
 * @returns {*}
 */
export const uploadImage = (itemId, file) => {
  let formData = new FormData()
  formData.append('file', file)

  return axios({
    url: `BlogPost/${itemId}/UploadImage`,
    method: 'post',
    headers: {'Content-Type': 'multipart/form-data;charset=UTF-8'},
    data: formData
  })
}

// Lấy các ảnh trong bài viết
export const getImages = itemId => {
  return axios({
    url: `BlogPost/${itemId}/Images`,
    method: 'get'
  })
}

// Đặt blog đề xuất
export const setFeatured = itemId => {
  return axios({
    url: `BlogPost/${itemId}/SetFeatured`,
    method: 'post'
  })
}

// Bỏ đề xuất blog
export const cancelFeatured = itemId => {
  return axios({
    url: `BlogPost/${itemId}/CancelFeatured`,
    method: 'post'
  })
}

// Đặt ghim
export const setTop = itemId => {
  return axios({
    url: `BlogPost/${itemId}/SetTop`,
    method: 'post'
  })
}
