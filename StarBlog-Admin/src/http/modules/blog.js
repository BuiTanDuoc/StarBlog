import axios from '../axios'

// Lấy blog được ghim
export const top = () => {
  return axios({
    url: 'Blog/Top',
    method: 'get'
  })
}

// Lấy blog đề xuất
export const featured = () => {
  return axios({
    url: 'Blog/Featured',
    method: 'get'
  })
}

// Lấy thông tin tổng quan blog
export const overview = () => {
  return axios({
    url: 'Blog/Overview',
  })
}

// Danh sách trạng thái bài viết blog
export const getStatusList = () => {
  return axios({
    url: 'Blog/GetStatusList'
  })
}

// Tải bài viết lên
export const upload = (title, summary, categoryId, file, zipEncoding = 'utf-8') => {
  let formData = new FormData()
  formData.append('title', title)
  formData.append('summary', summary)
  formData.append('categoryId', categoryId)
  formData.append('file', file)
  formData.append('zipEncoding', zipEncoding)

  return axios({
    url: 'Blog/Upload',
    method: 'post',
    headers: {'Content-Type': 'multipart/form-data;charset=UTF-8'},
    data: formData
  })
}
