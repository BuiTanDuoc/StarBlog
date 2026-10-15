// Import tất cả API
import api from './api'

const install = Vue => {
  if (install.installed)
    return;

  install.installed = true;

  Object.defineProperties(Vue.prototype, {
    // Lưu ý: ở đây được gắn vào đối tượng $api trên prototype của Vue
    $api: {
      get() {
        return api
      }
    }
  })
}

export default install
