import axios from "../axios"

// Lấy tất cả
export const getAll = () => {
  return axios({
    url: 'FeaturedCategory',
    method: 'get',
  })
}

// Lấy mục được chỉ định
export const get = itemId => {
  return axios({
    url: `FeaturedCategory/${itemId}`,
    method: 'get'
  })
}

// Thêm dự án
export const add = data => {
  return axios({
    url: 'FeaturedCategory',
    method: 'post',
    data
  })
}

// Xóa mục được chỉ định
export const deleteItem = itemId => {
  return axios({
    url: `FeaturedCategory/${itemId}/`,
    method: 'delete'
  })
}
