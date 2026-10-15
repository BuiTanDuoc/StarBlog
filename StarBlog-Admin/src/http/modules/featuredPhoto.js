import axios from "../axios"

// Lấy tất cả
export const getAll = () => {
  return axios({
    url: 'FeaturedPhoto',
    method: 'get',
  })
}

// Lấy mục được chỉ định
export const get = itemId => {
  return axios({
    url: `FeaturedPhoto/${itemId}`,
    method: 'get'
  })
}

// Thêm dự án
export const add = data => {
  return axios({
    url: 'FeaturedPhoto',
    method: 'post',
    data
  })
}

// Xóa mục được chỉ định
export const deleteItem = itemId => {
  return axios({
    url: `FeaturedPhoto/${itemId}/`,
    method: 'delete'
  })
}
