export default {
  state: {
    appName: "StarBlog Admin",  // Tên ứng dụng
    themeColor: "#1993DFFF",  // Màu chủ đề
    oldThemeColor: "#1993DFFF",   // Màu chủ đề lần trước
    collapse: false,  // Trạng thái thu gọn của thanh điều hướng
    menuRouteLoaded: false,    // Menu và route đã được tải hay chưa
    fullscreen: false,      // Trạng thái toàn màn hình
  },
  getters: {
    collapse(state) {   // Tương ứng với state ở trên
      return state.collapse
    },
    fullscreen(state) {
      return state.fullscreen
    }
  },
  mutations: {
    // Thay đổi trạng thái thu gọn
    onCollapse(state) {
      state.collapse = !state.collapse
    },
    // Đổi màu chủ đề
    setThemeColor(state, themeColor) {
      state.oldThemeColor = state.themeColor
      state.themeColor = themeColor
    },
    // Thay đổi trạng thái tải của menu và route
    menuRouteLoaded(state, menuRouteLoaded) {
      state.menuRouteLoaded = menuRouteLoaded;
    },
    // Chuyển đổi toàn màn hình
    onFullscreenChange(state) {
      state.fullscreen = !state.fullscreen
    }
  },
  actions: {}
}
