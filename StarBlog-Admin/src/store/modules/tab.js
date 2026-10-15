export default {
  state: {
    // Tab nhập chính
    mainTabs: [],
    // Tên tab hiện tại
    mainTabsActiveName: ''
  },
  mutations: {
    updateMainTabs(state, tabs) {
      state.mainTabs = tabs
    },
    updateMainTabsActiveName(state, name) {
      state.mainTabsActiveName = name
    }
  }
}
