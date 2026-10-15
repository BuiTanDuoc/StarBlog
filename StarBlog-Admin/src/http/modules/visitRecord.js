import axios from '../axios'

// Lấy danh sách
export const getList = (
  {
    excludeApi = false,
    excludeIntranetIp = false,
    country = '',
    province = '',
    city = '',
    isp = '',
    os = '',
    device = '',
    userAgent = '',
    isSpider = null,
    search = '',
    sortBy = '',
    page = 1,
    pageSize = 10,
  }) => {
  return axios({
    url: 'VisitRecord',
    method: 'get',
    params: {
      excludeApi, excludeIntranetIp,
      country, province, city, isp,
      os, device, userAgent, isSpider,
      search, sortBy, page, pageSize
    }
  })
}

// Lấy mục được chỉ định
export const get = id => {
  return axios({
    url: `VisitRecord/${id}/`,
    method: 'get',
  })
}

// Lấy tất cả các mục
export const getAll = () => {
  return axios({url: 'VisitRecord/All'})
}

// Lấy dữ liệu tổng quan
export const getOverview = () => {
  return axios({url: 'VisitRecord/Overview'})
}

// Lấy dữ liệu xu hướng
export const getTrend = (days = 7) => {
  return axios({
    url: 'VisitRecord/DailyTrend',
    method: 'get',
    params: {days}
  })
}

// Lấy dữ liệu thống kê
export const getStats = (date = null) => {
  return axios({
    url: 'VisitRecord/Stats',
    method: 'get',
    params: date === null ? null : {...date}
  })
}

export const getGeoFilterParams = (
  {
    param = 'country',
    country = '',
    province = '',
    city = '',
  }) => {
  return axios({
    url: 'VisitRecord/GeoFilterParams',
    params: {param, country, province, city},
  })
}

export const getUserAgentFilterParams = () => {
  return axios({
    url: 'VisitRecord/UserAgentFilterParams',
  })
}
