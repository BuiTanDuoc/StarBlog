import axios from "../axios"

export const getNodes = () => {
  return axios({
    url: `Category/Nodes`,
  })
}

// Lấy tất cả danh mục
export const getAll = () => {
  return axios({
    url: 'Category/All',
    method: 'get',
  })
}

export const getList = (page = 1, pageSize = 10) => {
  return axios({
    url: 'Category',
    method: 'get',
    params: { page, pageSize }
  })
}

export const getPosts = (itemId, page = 1, pageSize = 20) => {
  return axios({
    url: 'BlogPost',
    method: 'get',
    params: { categoryId: itemId, page, pageSize }
  })
}

// Lấy mục được chỉ định
export const get = itemId => {
  return axios({
    url: `Category/${itemId}`,
    method: 'get'
  })
}

// Thêm danh mục
export const add = data => {
  return axios({
    url: `Category`,
    method: 'post',
    data
  })
}

// Cập nhật danh mục
export const update = data => {
  return axios({
    url: `Category/${data.id}/`,
    method: 'put',
    data
  })
}

export const deleteItem = itemId => {
  return axios({
    url: `Category/${itemId}/`,
    method: 'delete'
  })
}

// Đám mây từ danh mục
export const wordCloud = () => {
  return axios({
    url: 'Category/WordCloud'
  })
}

// Đặt danh mục đề xuất
export const setFeatured = (itemId, data) => {
  return axios({
    url: `Category/${itemId}/SetFeatured`,
    method: 'post',
    data
  })
}

// Bỏ đề xuất danh mục
export const cancelFeatured = itemId => {
  return axios({
    url: `Category/${itemId}/CancelFeatured`,
    method: 'post'
  })
}

// Đặt thành hiển thị
export const setVisible = itemId => {
  return axios({
    url: `Category/${itemId}/SetVisible`,
    method: 'post',
  })
}

// Đặt thành không hiển thị
export const setInvisible = itemId => {
  return axios({
    url: `Category/${itemId}/SetInvisible`,
    method: 'post',
  })
}
